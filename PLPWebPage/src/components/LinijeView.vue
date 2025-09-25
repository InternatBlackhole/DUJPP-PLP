<template>
  <div class="lines-container">
    <div class="lines-header">
      <h2>Lines</h2>
      <button @click="refreshData" class="refresh-btn">Refresh Data</button>
    </div>

    <div class="lines-content">
      <p v-if="loading">Loading...</p>
      <p v-else-if="error" class="error-message">{{ error }}</p>
      <DataTable
        v-else
        :headers="tableHeaders"
        :data="pagedLines"
        :rowKey="row => (row as any).id"
        :page="page"
        :pageSize="pageSize"
        :total="lines.length"
        @update:page="onPageChange"
        @update:pageSize="onPageSizeChange"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useApi } from "@/composables/useApi";
import { useSessionStorage } from "@vueuse/core";
import { firstValueFrom } from "rxjs";
import DataTable from "./common/DataTable.vue";

const { linije } = useApi();

const loading = ref(false);
const error = ref("");
const page = ref(1);
const pageSize = ref(10);

interface SimpleLinija {
  id: string | undefined | null;
  ime: string | null | undefined;
}

// Session storage for caching
const lines = useSessionStorage<SimpleLinija[]>("linije-data", []);

// Table configuration
const tableHeaders = [
  { key: "id", label: "ID" },
  { key: "ime", label: "Name" },
];

const pagedLines = computed(() => {
  const start = (page.value - 1) * pageSize.value;
  return lines.value.slice(start, start + pageSize.value);
});

function onPageChange(newPage: number) {
  page.value = newPage;
}

function onPageSizeChange(newSize: number) {
  pageSize.value = newSize;
  page.value = 1;
}

const fetchLines = async () => {
  loading.value = true;
  error.value = "";

  try {
    const data = await firstValueFrom(linije.linijeGet({}));
    lines.value = data.map<SimpleLinija>((e) => ({
      id: e.id,
      ime: e.ime,
    }));
  } catch (err) {
    console.error("Failed to fetch lines:", err);
    error.value = "Failed to load lines data";
  } finally {
    loading.value = false;
  }
};

const refreshData = () => {
  fetchLines();
};

// Initial data fetch
onMounted(() => {
  if (lines.value.length === 0) {
    fetchLines();
  }
});
</script>

<style scoped>
.lines-container {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.lines-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.lines-header h2 {
  margin: 0;
}

.refresh-btn {
  background-color: #4caf50;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.refresh-btn:hover {
  background-color: #45a049;
}

.lines-content {
  margin-top: 1rem;
}

.error-message {
  color: #dc3545;
  text-align: center;
  padding: 1rem;
}
</style>
