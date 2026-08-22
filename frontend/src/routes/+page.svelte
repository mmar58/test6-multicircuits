<script lang="ts">
    import { goto } from "$app/navigation";
    import { signalrService } from "$lib/signalr";
    import { userStore } from "$lib/stores/user.svelte";
    import { onMount } from "svelte";
    import { Loader2 } from "@lucide/svelte";

    let name = $state("");
    let isConnecting = $state(false);
    let error = $state("");

    async function handleJoin(e: Event) {
        e.preventDefault();
        if (!name.trim()) {
            error = "Please enter a name.";
            return;
        }

        isConnecting = true;
        error = "";

        try {
            const connection = signalrService.init();
            if (connection.state === "Disconnected") {
                await connection.start();
            }

            const result = await connection.invoke("RegisterName", name.trim());
            userStore.id = result.id;
            userStore.name = result.displayName;
            userStore.color = result.color;
            userStore.isJoined = true;

            goto("/dashboard");
        } catch (err: any) {
            error = err.message || "Failed to connect to the server.";
            isConnecting = false;
        }
    }
</script>

<div
    class="min-h-screen flex items-center justify-center bg-background p-4 relative overflow-hidden"
>
    <!-- decorative background elements -->
    <div
        class="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-cyan-900/20 via-background to-background pointer-events-none"
    ></div>
    <div
        class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-cyan-500/10 rounded-full blur-[120px] pointer-events-none"
    ></div>

    <div
        class="w-full max-w-md bg-card/50 backdrop-blur-xl border border-border rounded-2xl shadow-2xl p-8 relative z-10 animate-in fade-in zoom-in duration-500"
    >
        <div class="text-center mb-8">
            <div
                class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-cyan-500/20 text-cyan-400 mb-4 ring-1 ring-cyan-500/50 shadow-[0_0_20px_rgba(6,182,212,0.3)]"
            >
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="32"
                    height="32"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    ><path
                        d="M18 10h2a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-8a2 2 0 0 1 2-2h2"
                    /><path d="M12 2v10" /><path d="m9 5 3-3 3 3" /><rect
                        width="8"
                        height="8"
                        x="8"
                        y="10"
                        rx="2"
                    /></svg
                >
            </div>
            <h1 class="text-3xl font-bold tracking-tight text-foreground">
                LogicFlow
            </h1>
            <p class="text-muted-foreground mt-2">
                Collaborative logic circuit design.
            </p>
        </div>

        <form onsubmit={handleJoin} class="space-y-4">
            <div class="space-y-2">
                <label for="name" class="text-sm font-medium text-foreground"
                    >Your Name</label
                >
                <input
                    type="text"
                    id="name"
                    bind:value={name}
                    placeholder="e.g. John"
                    class="w-full bg-background/50 border border-input rounded-lg px-4 py-3 text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-cyan-500/50 transition-all"
                    disabled={isConnecting}
                />
            </div>

            {#if error}
                <p class="text-sm text-destructive">{error}</p>
            {/if}

            <button
                type="submit"
                class="w-full bg-cyan-600 hover:bg-cyan-500 text-white font-medium rounded-lg px-4 py-3 flex items-center justify-center transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-[0_0_15px_rgba(6,182,212,0.4)]"
                disabled={isConnecting || !name.trim()}
            >
                {#if isConnecting}
                    <Loader2 class="w-5 h-5 animate-spin mr-2" />
                    Connecting...
                {:else}
                    Start Collaborating
                {/if}
            </button>
        </form>
    </div>
</div>
