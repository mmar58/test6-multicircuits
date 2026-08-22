<script lang="ts">
    import { onMount, onDestroy } from "svelte";
    import { goto } from "$app/navigation";
    import { signalrService } from "$lib/signalr";
    import { dashboardStore } from "$lib/stores/dashboard.svelte";
    import { userStore } from "$lib/stores/user.svelte";
    import { LogOut, Plus, Search, CircuitBoard } from "@lucide/svelte";

    let isCreating = $state(false);
    let newCircuitName = $state("");
    let newCircuitDesc = $state("");

    onMount(async () => {
        if (!userStore.isJoined) {
            goto("/");
            return;
        }

        const conn = signalrService.getConnection();
        if (conn && conn.state === "Connected") {
            await conn.invoke("JoinDashboard");
        }
    });

    onDestroy(async () => {
        const conn = signalrService.getConnection();
        if (conn && conn.state === "Connected") {
            try {
                await conn.invoke("LeaveDashboard");
            } catch (e) {
                // Ignore if already disconnected
            }
        }
    });

    async function handleCreateCircuit() {
        if (!newCircuitName.trim()) return;

        const conn = signalrService.getConnection();
        if (conn) {
            const circuit = await conn.invoke(
                "CreateCircuit",
                newCircuitName,
                newCircuitDesc,
                20,
            );
            goto(`/circuit/${circuit.id}`);
        }
    }

    function logout() {
        const conn = signalrService.getConnection();
        if (conn) {
            conn.stop();
        }
        userStore.isJoined = false;
        goto("/");
    }
</script>

<div class="min-h-screen bg-background">
    <!-- Header -->
    <header
        class="border-b border-border bg-card/50 backdrop-blur-md sticky top-0 z-30"
    >
        <div
            class="container mx-auto px-4 h-16 flex items-center justify-between"
        >
            <div class="flex items-center gap-2 text-cyan-400">
                <CircuitBoard class="w-6 h-6" />
                <span class="font-bold text-xl tracking-tight text-foreground"
                    >LogicFlow</span
                >
            </div>

            <div class="flex items-center gap-4">
                <div
                    class="flex items-center gap-3 px-3 py-1.5 rounded-full bg-secondary/50 border border-border"
                >
                    <div
                        class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-sm"
                        style="background-color: {userStore.color}"
                    >
                        {userStore.name.substring(0, 2).toUpperCase()}
                    </div>
                    <span class="text-sm font-medium">{userStore.name}</span>
                </div>

                <button
                    onclick={logout}
                    class="text-muted-foreground hover:text-foreground transition-colors p-2 rounded-md hover:bg-secondary"
                >
                    <LogOut class="w-5 h-5" />
                </button>
            </div>
        </div>
    </header>

    <!-- Main Content -->
    <main class="container mx-auto px-4 py-8">
        <div
            class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8"
        >
            <div>
                <h1 class="text-3xl font-bold tracking-tight">
                    Circuit Projects
                </h1>
                <p class="text-muted-foreground mt-1">
                    Collaborate with your team in real-time.
                </p>
            </div>

            <button
                onclick={() => (isCreating = true)}
                class="bg-cyan-600 hover:bg-cyan-500 text-white font-medium rounded-lg px-4 py-2.5 flex items-center gap-2 transition-all shadow-[0_0_15px_rgba(6,182,212,0.3)]"
            >
                <Plus class="w-5 h-5" />
                New Circuit
            </button>
        </div>

        {#if dashboardStore.circuits.length === 0}
            <div
                class="text-center py-20 border-2 border-dashed border-border rounded-2xl bg-card/20"
            >
                <div
                    class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-secondary mb-4"
                >
                    <CircuitBoard class="w-8 h-8 text-muted-foreground" />
                </div>
                <h3 class="text-xl font-semibold mb-2">No circuits yet</h3>
                <p class="text-muted-foreground max-w-sm mx-auto mb-6">
                    Create your first logic circuit project to start
                    collaborating with others.
                </p>
                <button
                    onclick={() => (isCreating = true)}
                    class="bg-primary hover:bg-primary/90 text-primary-foreground font-medium rounded-lg px-6 py-2.5"
                >
                    Create Project
                </button>
            </div>
        {:else}
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {#each dashboardStore.circuits as circuit}
                    <div
                        class="bg-card border border-border rounded-xl p-5 hover:border-cyan-500/50 hover:shadow-[0_0_20px_rgba(6,182,212,0.1)] transition-all group flex flex-col h-full cursor-pointer"
                        onclick={() => goto(`/circuit/${circuit.id}`)}
                    >
                        <div class="flex justify-between items-start mb-4">
                            <h3
                                class="text-lg font-semibold group-hover:text-cyan-400 transition-colors line-clamp-1"
                            >
                                {circuit.name}
                            </h3>
                            <span
                                class="text-xs font-medium bg-secondary px-2 py-1 rounded-md text-muted-foreground whitespace-nowrap"
                            >
                                Grid: {circuit.gridSize}
                            </span>
                        </div>

                        <p
                            class="text-muted-foreground text-sm flex-grow line-clamp-2 mb-6"
                        >
                            {circuit.description || "No description provided."}
                        </p>

                        <div
                            class="flex items-center justify-between mt-auto pt-4 border-t border-border/50"
                        >
                            <div class="flex items-center -space-x-2">
                                {#if circuit.activeUserIds && circuit.activeUserIds.length > 0}
                                    {#each circuit.activeUserIds.slice(0, 3) as userId}
                                        {@const user =
                                            dashboardStore.onlineUsers.find(
                                                (u) => u.id === userId,
                                            )}
                                        {#if user}
                                            <div
                                                class="w-8 h-8 rounded-full border-2 border-card flex items-center justify-center text-xs font-bold text-white shadow-sm"
                                                style="background-color: {user.color}"
                                                title={user.displayName}
                                            >
                                                {user.displayName
                                                    .substring(0, 2)
                                                    .toUpperCase()}
                                            </div>
                                        {/if}
                                    {/each}
                                    {#if circuit.activeUserIds.length > 3}
                                        <div
                                            class="w-8 h-8 rounded-full border-2 border-card bg-secondary flex items-center justify-center text-xs font-medium text-muted-foreground"
                                        >
                                            +{circuit.activeUserIds.length - 3}
                                        </div>
                                    {/if}
                                {:else}
                                    <span class="text-xs text-muted-foreground"
                                        >No active users</span
                                    >
                                {/if}
                            </div>

                            <div class="flex items-center gap-2">
                                <button
                                    onclick={(e) => {
                                        e.stopPropagation();
                                        if (confirm('Are you sure you want to delete this circuit?')) {
                                            const conn = signalrService.getConnection();
                                            if (conn) conn.invoke('DeleteCircuit', circuit.id);
                                        }
                                    }}
                                    class="p-2 text-muted-foreground hover:text-red-400 hover:bg-red-400/10 rounded-md transition-colors opacity-0 group-hover:opacity-100"
                                    title="Delete Circuit"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                                </button>
                                <span
                                    class="text-sm font-medium text-cyan-500 opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-1"
                                >
                                    Open <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="16"
                                        height="16"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        stroke-width="2"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                        ><path d="M5 12h14" /><path
                                            d="m12 5 7 7-7 7"
                                        /></svg
                                    >
                                </span>
                            </div>
                        </div>
                    </div>
                {/each}
            </div>
        {/if}
    </main>

    <!-- Create Modal -->
    {#if isCreating}
        <div
            class="fixed inset-0 bg-background/80 backdrop-blur-sm z-50 flex items-center justify-center p-4"
        >
            <div
                class="bg-card border border-border rounded-xl shadow-2xl w-full max-w-md overflow-hidden animate-in zoom-in-95 duration-200"
            >
                <div
                    class="px-6 py-4 border-b border-border flex justify-between items-center"
                >
                    <h2 class="text-lg font-semibold">New Circuit Project</h2>
                    <button
                        onclick={() => (isCreating = false)}
                        class="text-muted-foreground hover:text-foreground"
                    >
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="20"
                            height="20"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            ><path d="M18 6 6 18" /><path d="m6 6 12 12" /></svg
                        >
                    </button>
                </div>
                <div class="p-6 space-y-4">
                    <div class="space-y-2">
                        <label for="cname" class="text-sm font-medium"
                            >Circuit Name</label
                        >
                        <input
                            id="cname"
                            type="text"
                            bind:value={newCircuitName}
                            class="w-full bg-background border border-input rounded-md px-3 py-2"
                            placeholder="e.g. 8-bit Adder"
                        />
                    </div>
                    <div class="space-y-2">
                        <label for="cdesc" class="text-sm font-medium"
                            >Description (Optional)</label
                        >
                        <textarea
                            id="cdesc"
                            bind:value={newCircuitDesc}
                            class="w-full bg-background border border-input rounded-md px-3 py-2 min-h-[100px] resize-none"
                            placeholder="What does this circuit do?"
                        ></textarea>
                    </div>
                </div>
                <div
                    class="px-6 py-4 bg-secondary/50 border-t border-border flex justify-end gap-2"
                >
                    <button
                        onclick={() => (isCreating = false)}
                        class="px-4 py-2 text-sm font-medium hover:bg-secondary rounded-md transition-colors"
                        >Cancel</button
                    >
                    <button
                        onclick={handleCreateCircuit}
                        disabled={!newCircuitName.trim()}
                        class="px-4 py-2 bg-cyan-600 hover:bg-cyan-500 text-white text-sm font-medium rounded-md transition-colors disabled:opacity-50"
                        >Create Project</button
                    >
                </div>
            </div>
        </div>
    {/if}
</div>
