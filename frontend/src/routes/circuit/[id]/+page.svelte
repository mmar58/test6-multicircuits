<script lang="ts">
    import { page } from "$app/stores";
    import { onMount, onDestroy } from "svelte";
    import { goto } from "$app/navigation";
    import { signalrService } from "$lib/signalr";
    import { userStore } from "$lib/stores/user.svelte";
    import { circuitStore } from "$lib/stores/circuit.svelte";
    import { dashboardStore } from "$lib/stores/dashboard.svelte";
    import {
        ArrowLeft,
        Settings,
        Play,
        Download,
        Table2,
        Image,
    } from "@lucide/svelte";
    import CircuitCanvas from "$lib/components/circuit/CircuitCanvas.svelte";
    import GatePalette from "$lib/components/circuit/GatePalette.svelte";
    import TruthTablePanel from "$lib/components/circuit/TruthTablePanel.svelte";
    import { exportToPdf, exportToPng } from "$lib/circuit/exporter";

    let circuitId = $page.params.id;
    let circuit = $derived(
        dashboardStore.circuits.find((c) => c.id === circuitId),
    );
    let isSimulating = $state(false);
    let showTruthTable = $state(false);
    let showToolsMenu = $state(false);

    let canvasComponent: any = $state(null);

    onMount(async () => {
        if (!userStore.isJoined) {
            goto("/");
            return;
        }

        circuitStore.id = circuitId;
        const joinedCircuit = await signalrService.invoke<any>("JoinCircuit", circuitId);
        if (!joinedCircuit) {
            goto("/dashboard");
            return;
        }
        circuitStore.elements = joinedCircuit.elements;
        circuitStore.wires = joinedCircuit.wires;
        circuitStore.activeUserIds = joinedCircuit.activeUserIds;
    });

    onDestroy(async () => {
        if (circuitStore.id) {
            try {
                await signalrService.invoke("LeaveCircuit", circuitStore.id);
            } catch (e) {}
        }
        circuitStore.id = null;
        circuitStore.elements = [];
        circuitStore.wires = [];
        circuitStore.cursors = {};
    });

    function handleExportPdf() {
        if (canvasComponent && circuit) {
            exportToPdf(
                canvasComponent.getSvgElement(),
                circuit.name || "Untitled Circuit",
            );
        }
    }

    function handleExportPng() {
        if (canvasComponent && circuit) {
            exportToPng(
                canvasComponent.getSvgElement(),
                circuit.name || "Untitled Circuit",
            );
        }
    }
</script>

<svelte:window onclick={() => (showToolsMenu = false)} />

<div class="h-screen w-screen flex flex-col overflow-hidden bg-[#0a0a0f]">
    <!-- Top Bar -->
    <header
        class="h-14 bg-card/80 backdrop-blur-md border-b border-border flex items-center justify-between px-4 z-40 shrink-0"
    >
        <div class="flex items-center gap-4">
            <button
                onclick={() => goto("/dashboard")}
                class="text-muted-foreground hover:text-foreground p-2 rounded-md hover:bg-secondary transition-colors"
            >
                <ArrowLeft class="w-5 h-5" />
            </button>
            <div>
                <h1 class="font-semibold tracking-tight leading-tight">
                    {circuit?.name || "Loading..."}
                </h1>
                <p class="text-xs text-muted-foreground leading-tight">
                    Grid: {circuit?.gridSize || 20}px
                </p>
            </div>
        </div>

        <div class="flex items-center gap-3">
            <div class="flex -space-x-2 mr-4">
                {#each circuitStore.activeUserIds as userId}
                    {@const u =
                        dashboardStore.onlineUsers.find(
                            (ou) => ou.id === userId,
                        ) || (userId === userStore.id ? userStore : null)}
                    {#if u}
                        <div
                            class="w-7 h-7 rounded-full border-2 border-card flex items-center justify-center text-[10px] font-bold text-white shadow-sm"
                            style="background-color: {u.color}"
                            title={u.displayName || u.name}
                        >
                            {(u.displayName || u.name)
                                .substring(0, 2)
                                .toUpperCase()}
                        </div>
                    {/if}
                {/each}
            </div>

            <div class="h-6 w-px bg-border mx-1"></div>
            <!-- Tools -->
            <div class="relative">
                <button
                    onclick={(e) => {
                        e.stopPropagation();
                        showToolsMenu = !showToolsMenu;
                    }}
                    class="flex items-center gap-2 px-4 py-1.5 text-sm font-medium text-slate-200 bg-slate-800 hover:bg-slate-700 rounded-md transition-colors border border-slate-700"
                >
                    Tools
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        class="transition-transform {showToolsMenu
                            ? 'rotate-180'
                            : ''}"><path d="m6 9 6 6 6-6" /></svg
                    >
                </button>

                {#if showToolsMenu}
                    <div
                        class="absolute right-0 mt-2 w-48 bg-slate-900 border border-slate-700 shadow-xl rounded-md overflow-hidden z-50 animate-in fade-in slide-in-from-top-2 duration-100"
                    >
                        <button
                            onclick={() => {
                                showTruthTable = !showTruthTable;
                                showToolsMenu = false;
                            }}
                            class="w-full text-left px-4 py-2.5 text-sm hover:bg-slate-800 hover:text-cyan-400 transition-colors flex items-center gap-2 {showTruthTable
                                ? 'text-cyan-400'
                                : 'text-slate-200'}"
                        >
                            <Table2 class="w-4 h-4" /> Truth Table
                        </button>
                        <div class="h-px w-full bg-slate-700"></div>
                        <button
                            onclick={() => {
                                handleExportPng();
                                showToolsMenu = false;
                            }}
                            class="w-full text-left px-4 py-2.5 text-sm hover:bg-slate-800 hover:text-cyan-400 transition-colors flex items-center gap-2 text-slate-200"
                        >
                            <Image class="w-4 h-4" /> Export as PNG
                        </button>
                        <button
                            onclick={() => {
                                handleExportPdf();
                                showToolsMenu = false;
                            }}
                            class="w-full text-left px-4 py-2.5 text-sm hover:bg-slate-800 hover:text-cyan-400 transition-colors flex items-center gap-2 text-slate-200"
                        >
                            <Download class="w-4 h-4" /> Print to PDF
                        </button>
                    </div>
                {/if}
            </div>
            <button
                class="p-2 text-muted-foreground hover:text-foreground rounded-md hover:bg-secondary transition-colors"
                title="Settings"
            >
                <Settings class="w-5 h-5" />
            </button>

            <button
                onclick={() => (isSimulating = !isSimulating)}
                class="flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-colors {isSimulating
                    ? 'bg-green-500/20 text-green-400 border border-green-500/50'
                    : 'bg-secondary text-foreground hover:bg-secondary/80 border border-transparent'}"
            >
                <Play class="w-4 h-4 {isSimulating ? 'fill-green-400' : ''}" />
                {isSimulating ? "Simulating" : "Simulate"}
            </button>
        </div>
    </header>

    <div class="flex-1 flex overflow-hidden relative">
        <GatePalette />

        <div class="flex-1 relative cursor-crosshair">
            <CircuitCanvas {isSimulating} bind:this={canvasComponent} />
        </div>

        {#if showTruthTable}
            <TruthTablePanel
                elements={circuitStore.elements}
                wires={circuitStore.wires}
                onClose={() => (showTruthTable = false)}
            />
        {/if}
    </div>
</div>
