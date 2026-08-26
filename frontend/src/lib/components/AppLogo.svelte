<script lang="ts">
    import { CircuitBoard } from "@lucide/svelte";

    let { onclick } = $props<{ onclick?: () => void }>();
</script>

{#if onclick}
    <button
        {onclick}
        class="flex items-center gap-2 text-cyan-400 hover:text-cyan-300 transition-colors"
    >
        <CircuitBoard class="w-6 h-6" />
        <span class="font-bold text-xl tracking-tight text-foreground">LogicFlow</span>
    </button>
{:else}
    <div class="flex items-center gap-2 text-cyan-400">
        <CircuitBoard class="w-6 h-6" />
        <span class="font-bold text-xl tracking-tight text-foreground">LogicFlow</span>
    </div>
{/if}
