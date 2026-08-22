    <script lang="ts">
        import { page } from "$app/stores";
        import { onMount, onDestroy } from "svelte";
        import { goto } from "$app/navigation";
        import { signalrService } from "$lib/signalr";
        import { userStore } from "$lib/stores/user.svelte";
        import { circuitStore } from "$lib/stores/circuit.svelte";
        import { dashboardStore } from "$lib/stores/dashboard.svelte";
        import { ArrowLeft, Settings, Play, Download, Table2, Image } from "@lucide/svelte";
        import CircuitCanvas from "$lib/components/circuit/CircuitCanvas.svelte";
        import GatePalette from "$lib/components/circuit/GatePalette.svelte";
        import TruthTablePanel from "$lib/components/circuit/TruthTablePanel.svelte";
        import { exportToPdf, exportToPng } from "$lib/circuit/exporter";
        
        let circuitId = $page.params.id;
        let circuit = $derived(dashboardStore.circuits.find(c => c.id === circuitId));
        let isSimulating = $state(false);
        let showTruthTable = $state(false);
        
        let canvasComponent: any = $state(null);
        
        onMount(async () => {
            if (!userStore.isJoined) {
                goto("/");
                return;
            }
            
            circuitStore.id = circuitId;
            const conn = signalrService.getConnection();
            if (conn && conn.state === "Connected") {
                const joinedCircuit = await conn.invoke("JoinCircuit", circuitId);
                if (!joinedCircuit) {
                    goto("/dashboard");
                    return;
                }
                circuitStore.elements = joinedCircuit.elements;
                circuitStore.wires = joinedCircuit.wires;
                circuitStore.activeUserIds = joinedCircuit.activeUserIds;
            }
        });
        
        onDestroy(async () => {
            const conn = signalrService.getConnection();
            if (conn && conn.state === "Connected" && circuitStore.id) {
                try {
                    await conn.invoke("LeaveCircuit", circuitStore.id);
                } catch (e) {}
            }
            circuitStore.id = null;
            circuitStore.elements = [];
            circuitStore.wires = [];
            circuitStore.cursors = {};
        });
        
        function handleExportPdf() {
            if (canvasComponent && circuit) {
                exportToPdf(canvasComponent.getSvgElement(), circuit.name || "Untitled Circuit");
            }
        }
        
        function handleExportPng() {
            if (canvasComponent && circuit) {
                exportToPng(canvasComponent.getSvgElement(), circuit.name || "Untitled Circuit");
            }
        }
    </script>
    
    <div class="h-screen w-screen flex flex-col overflow-hidden bg-[#0a0a0f]">
        <!-- Top Bar -->
        <header class="h-14 bg-card/80 backdrop-blur-md border-b border-border flex items-center justify-between px-4 z-20 shrink-0">
            <div class="flex items-center gap-4">
                <button onclick={() => goto("/dashboard")} class="text-muted-foreground hover:text-foreground p-2 rounded-md hover:bg-secondary transition-colors">
                    <ArrowLeft class="w-5 h-5" />
                </button>
                <div>
                    <h1 class="font-semibold tracking-tight leading-tight">{circuit?.name || "Loading..."}</h1>
                    <p class="text-xs text-muted-foreground leading-tight">Grid: {circuit?.gridSize || 20}px</p>
                </div>
            </div>
            
            <div class="flex items-center gap-3">
                <div class="flex -space-x-2 mr-4">
                    {#each circuitStore.activeUserIds as userId}
                        {@const u = dashboardStore.onlineUsers.find(ou => ou.id === userId) || (userId === userStore.id ? userStore : null)}
                        {#if u}
                            <div class="w-7 h-7 rounded-full border-2 border-card flex items-center justify-center text-[10px] font-bold text-white shadow-sm" style="background-color: {u.color}" title={u.displayName || u.name}>
                                {(u.displayName || u.name).substring(0, 2).toUpperCase()}
                            </div>
                        {/if}
                    {/each}
                </div>
                
                <div class="h-6 w-px bg-border mx-1"></div>
                
                <button onclick={handleExportPng} class="p-2 text-muted-foreground hover:text-cyan-400 rounded-md hover:bg-secondary transition-colors" title="Export PNG">
                    <Image class="w-5 h-5" />
                </button>
                <button onclick={handleExportPdf} class="p-2 text-muted-foreground hover:text-cyan-400 rounded-md hover:bg-secondary transition-colors" title="Export PDF">
                    <Download class="w-5 h-5" />
                </button>
                <button onclick={() => showTruthTable = true} class="p-2 text-muted-foreground hover:text-cyan-400 rounded-md hover:bg-secondary transition-colors {showTruthTable ? 'text-cyan-400 bg-secondary' : ''}" title="Truth Table">
                    <Table2 class="w-5 h-5" />
                </button>
                <button class="p-2 text-muted-foreground hover:text-foreground rounded-md hover:bg-secondary transition-colors" title="Settings">
                    <Settings class="w-5 h-5" />
                </button>
                
                <button 
                    onclick={() => isSimulating = !isSimulating}
                    class="flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-colors {isSimulating ? 'bg-green-500/20 text-green-400 border border-green-500/50' : 'bg-secondary text-foreground hover:bg-secondary/80 border border-transparent'}"
                >
                    <Play class="w-4 h-4 {isSimulating ? 'fill-green-400' : ''}" />
                    {isSimulating ? 'Simulating' : 'Simulate'}
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
                    onClose={() => showTruthTable = false} 
                />
            {/if}
        </div>
    </div>
