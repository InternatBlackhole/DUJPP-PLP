<template>
  <div class="contracts-container">
    <h2>Pogodbe</h2>
    <div class="contracts-content">
      <p v-if="loading">Nalagam...</p>
      <p v-else-if="error" class="error-message">{{ error }}</p>
      <DataTable
        v-else
        :headers="tableHeaders"
        :data="pagedContracts"
        :rowKey="(row) => (row as any).id"
        :page="page"
        :pageSize="pageSize"
        :total="contracts.length"
        @update:page="onPageChange"
        @update:pageSize="onPageSizeChange"
      >
        <template #header="{ headers }">
          <th v-for="header in headers" :key="header.key">{{ header.label }}</th>
          <th>Prenos datotek</th>
        </template>
        <template #row="{ row }">
          <td v-for="header in tableHeaders" :key="header.key">
            {{ (row as any)[header.key] }}
          </td>
          <td>
            <input class="downloadBtn" type="image" src="/PDF_icon.svg" @click="downloadContract(row.id)" />
          </td>
        </template>
      </DataTable>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useApi } from "@/composables/useApi";
import { firstValueFrom } from "rxjs";
import DataTable from "./common/DataTable.vue";
import type { PogodbaBasic } from "@/api_wrapper/models";

const { pogodbe } = useApi();
const loading = ref(false);
const error = ref("");
const contracts = ref<PogodbaBasic[]>([]);
const page = ref(1);
const pageSize = ref(10);

const tableHeaders = [
  { key: "id", label: "ID" },
  { key: "linijaId", label: "ID Linije" },
  { key: "znesek", label: "Znesek" },
];

const pagedContracts = computed(() => {
  const start = (page.value - 1) * pageSize.value;
  return contracts.value.slice(start, start + pageSize.value);
});

function onPageChange(newPage: number) {
  page.value = newPage;
}

function onPageSizeChange(newSize: number) {
  pageSize.value = newSize;
  page.value = 1;
}

function downloadContract(id: string | undefined) {
  alert(`TODO: Prenos pogodbe z id ${id}`)
}

const fetchContracts = async () => {
  loading.value = true;
  error.value = "";
  try {
    const data = await firstValueFrom(pogodbe.pogodbeGet({}));
    contracts.value = data;
  } catch (err) {
    error.value = "Failed to load contracts data";
    console.error("Failed to fetch contracts:", err);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchContracts();
});
</script>

<style scoped>
.contracts-container {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.contracts-content {
  margin-top: 1rem;
}

.error-message {
  color: #dc3545;
  text-align: center;
  padding: 1rem;
}

.downloadBtn {
  width: 24px;
  height: 24px;
}
</style>
