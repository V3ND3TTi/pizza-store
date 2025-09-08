<script lang="ts">
  import { onMount } from "svelte";
  import { slide } from "svelte/transition";

  interface Pizza {
    id: number;
    name: string;
    description: string;
  }

  let pizzas: Pizza[] = [];
  let newPizzaName = "";
  let newPizzaDescription = "";
  let editingId: number | null = null;

  onMount(async () => {
    const res = await fetch("/pizzas");
    pizzas = await res.json();
  });

  function toggleEdit(id: number) {
    editingId = editingId === id ? null : id;
  }

  async function addPizza() {
    if (!newPizzaName.trim() || !newPizzaDescription.trim()) return;

    const res = await fetch("/pizzas", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: newPizzaName,
        description: newPizzaDescription,
      }),
    });

    if (res.ok) {
      const created: Pizza = await res.json();
      pizzas = [...pizzas, created];
      newPizzaName = "";
      newPizzaDescription = "";
    }
  }

  async function updatePizza(pizza: Pizza) {
    const res = await fetch(`/pizzas/${pizza.id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(pizza),
    });

    if (res.ok) {
      const updated: Pizza = await res.json();
      pizzas = pizzas.map((p) => (p.id === updated.id ? updated : p));
      editingId = null; // close accordion
    }
  }

  async function deletePizza(id: number) {
    const res = await fetch(`/pizzas/${id}`, {
      method: "DELETE",
    });
    if (res.ok) {
      pizzas = pizzas.filter((p) => p.id !== id);
    }
  }
</script>

<div class="dashboard">
  <h1>Admin Dashboard - Edit Menu</h1>

  <table>
    <thead>
      <tr>
        <th>ID</th>
        <th>Pizza Name</th>
        <th>Description</th>
        <th>Actions</th>
      </tr>
    </thead>
    <tbody>
      {#each pizzas as pizza, i}
        <tr>
          <td>{i + 1}</td>
          <td><strong>{pizza.name}</strong></td>
          <td>{pizza.description}</td>
          <td class="actions">
            <button class="edit-btn" on:click={() => toggleEdit(pizza.id)}
              >Edit</button
            >
            <button class="delete-btn" on:click={() => deletePizza(pizza.id)}
              >Delete</button
            >
          </td>
        </tr>

        {#if editingId === pizza.id}
          <tr class="edit-row">
            <td colspan="4">
              <div transition:slide={{ duration: 300 }}>
                <div class="accordion">
                  <input
                    type="text"
                    bind:value={pizza.name}
                    placeholder="Edit pizza name..."
                  />
                  <input
                    type="text"
                    bind:value={pizza.description}
                    placeholder="Edit pizza description..."
                  />
                  <button on:click={() => updatePizza(pizza)}>Save</button>
                  <button on:click={() => toggleEdit(pizza.id)}>Cancel</button>
                </div>
              </div>
            </td>
          </tr>
        {/if}
      {/each}
    </tbody>
  </table>

  <div class="form-row">
    <input
      type="text"
      bind:value={newPizzaName}
      placeholder="New pizza name..."
    />
    <input
      type="text"
      bind:value={newPizzaDescription}
      placeholder="New pizza description..."
    />
    <button on:click={addPizza}>Add Pizza</button>
  </div>
</div>

<style>
  .dashboard {
    max-width: 800px;
    margin: 2rem auto;
    padding: 1rem;
    background: #1e1e1e;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
  }

  h1 {
    text-align: center;
    margin-bottom: 1.5rem;
  }

  table {
    width: 100%;
    border-collapse: collapse;
  }

  th,
  td {
    padding: 0.75rem;
    border-bottom: 1px solid #333;
    text-align: left;
  }

  th {
    background: #2c2c2c;
  }

  tr:hover {
    background: #2a2a2a;
  }

  .actions {
    display: flex;
    gap: 0.5rem;
  }

  .delete-btn {
    background: #ff4d4d;
    border: none;
    color: white;
    padding: 0.4rem 0.75rem;
    border-radius: 6px;
    cursor: pointer;
    font-weight: bold;
  }

  .delete-btn:hover {
    background: #e63e3e;
  }

  .form-row {
    display: flex;
    gap: 0.5rem;
    margin-top: 1rem;
  }

  .form-row input {
    flex: 1;
    padding: 0.5rem;
    border-radius: 6px;
    border: none;
    background: #2c2c2c;
    color: #fff;
  }

  .form-row button {
    padding: 0.5rem 1rem;
    border-radius: 6px;
    border: none;
    background: #4caf50;
    color: #fff;
    cursor: pointer;
    font-weight: bold;
  }

  .form-row button:hover {
    background: #3d8c40;
  }

  .edit-row {
    background: #252525;
  }

  .accordion {
    display: flex;
    gap: 0.5rem;
    align-items: center;
  }

  .accordion input {
    flex: 1;
    padding: 0.5rem;
    border-radius: 6px;
    border: none;
    background: #333;
    color: #fff;
  }

  .accordion button {
    padding: 0.5rem 1rem;
    border-radius: 6px;
    border: none;
    cursor: pointer;
  }

  .accordion button:first-of-type {
    background: #4caf50;
    color: #fff;
  }

  .accordion button:last-of-type {
    background: #777;
    color: #fff;
  }
</style>
