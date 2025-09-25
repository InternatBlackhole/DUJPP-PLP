<template>
  <div class="records-container">
    <h2>Records</h2>
    <div class="records-content">
      <p v-if="loading">Loading...</p>
      <p v-else-if="error" class="error-message">{{ error }}</p>
      <DataTable
        v-else
        :headers="tableHeaders"
        :data="records"
        :rowKey="rowKey"
        :page="page"
        :pageSize="pageSize"
        :total="total"
        @update:page="onPageChange"
        @update:pageSize="onPageSizeChange"
        v-bind="{}"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useApi } from "@/composables/useApi";
import { firstValueFrom } from "rxjs";
import DataTable from "./common/DataTable.vue";
import type { MainPageZapis } from "@/api_wrapper/models";
import { useRoute, useRouter } from "vue-router";

const { zapisi } = useApi();
const loading = ref(false);
const error = ref("");
const records = ref<MainPageZapis[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(10);

const route = useRoute();
const router = useRouter();

const tableHeaders = [
  { key: "zacetekVoznje", label: "Start" },
  { key: "konecVoznje", label: "End" },
  { key: "linijaId", label: "Line ID" },
  { key: "nazivLinije", label: "Line Name" },
  { key: "narocnikId", label: "Client ID" },
  { key: "nazivNarocnika", label: "Client Name" },
  { key: "znesekPogodbe", label: "Contract Amount" },
];

const rowKey = (row: MainPageZapis) => row.zacetekVoznje + "-" + row.linijaId;

const fetchRecords = async () => {
  loading.value = true;
  error.value = "";
  try {
    const startIndex = (page.value - 1) * pageSize.value;
    const data = await firstValueFrom(
      zapisi.zapisiOptimizedGet({ startIndex, limit: pageSize.value })
    );
    records.value = data;
  } catch (err) {
    error.value = "Failed to load records data";
    console.error("Failed to fetch records:", err);
  } finally {
    loading.value = false;
  }
};

function syncFromRoute() {
  const q = route.query;
  page.value = q.page ? parseInt(q.page as string) : 1;
  pageSize.value = q.pageSize ? parseInt(q.pageSize as string) : 10;
}

function syncToRoute() {
  router.replace({
    query: { ...route.query, page: page.value, pageSize: pageSize.value },
  });
}

function onPageChange(newPage: number) {
  page.value = newPage;
}

function onPageSizeChange(newSize: number) {
  pageSize.value = newSize;
  page.value = 1;
}

watch([page, pageSize], () => {
  syncToRoute();
  fetchRecords();
});

onMounted(() => {
  syncFromRoute();
  fetchRecords();
});
</script>

<style scoped>
.records-container {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.records-content {
  margin-top: 1rem;
}

.error-message {
  color: #dc3545;
  text-align: center;
  padding: 1rem;
}
</style>
