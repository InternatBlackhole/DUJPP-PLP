<template>
  <table class="data-table">
    <thead>
      <tr>
        <slot name="header" :headers="headers" />
      </tr>
    </thead>
    <tbody>
      <tr v-for="(row, rowIndex) in data" :key="props.rowKey(row)">
        <slot name="row" :row="row" :rowIndex="rowIndex" :headers="headers" />
      </tr>
    </tbody>
  </table>
  <div>
    <div class="pagination-controls">
      <button :disabled="page <= 1" @click="$emit('update:page', page - 1)">&lt;</button>
      <span>Stran {{ page }} od {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="$emit('update:page', page + 1)">&gt;</button>
      <span class="page-size-select">
        Vnosov na stran:
        <select :value="pageSize" @change="onPageSizeChange">
          <option v-for="size in pageSizeOptions" :key="size" :value="size">{{ size }}</option>
        </select>
      </span>
      <span v-if="showTotal">({{ props.total }} skupaj)</span>
    </div>
  </div>
</template>

<script setup lang="ts" generic="T = object">
import { computed } from "vue";
const emit = defineEmits<{
  (e: "update:page", value: number): void;
  (e: "update:pageSize", value: number): void;
}>();

function onPageSizeChange(event: Event) {
  const target = event.target as HTMLSelectElement | null;
  if (target && target.value) {
    const value = Number(target.value);
    if (!isNaN(value)) {
      emit("update:pageSize", value);
    }
  }
}
interface TableHeader {
  key: string;
  label: string;
}

type DataTableProps<T> = {
  headers: TableHeader[];
  data: T[];
  rowKey: (row: T) => string | number;
  page: number;
  pageSize: number;
  total?: number;
  pageSizeOptions?: number[];
};

const props = defineProps<DataTableProps<T>>();

const page = computed(() => props.page);
const pageSize = computed(() => props.pageSize);
const totalPages = computed(() =>
  props.total !== undefined ? Math.max(1, Math.ceil(props.total / props.pageSize)) : 1
);
const pageSizeOptions = computed(() => props.pageSizeOptions ?? [10, 20, 50, 100]);
const showTotal = computed(() => typeof props.total === "number" && props.total > 0);
</script>

<style scoped>
.data-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

:slotted(.data-table th),
:slotted(.data-table td) {
  padding: 18px 8px;
  text-align: left;
  border-bottom: 1px solid rgba(0, 0, 0, 0.8);
}

:slotted(.data-table th) {
  font-weight: 600;
  background-color: #f8f9fa;
  border-bottom: 2px solid rgba(0, 0, 0, 0.7);
}

.data-table tbody tr:hover {
  background-color: #f5f5f5;
}
.pagination-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-top: 1rem;
}

.pagination-controls button {
  padding: 4px 10px;
  border: 1px solid #ccc;
  background: #fff;
  border-radius: 4px;
  cursor: pointer;
}
.pagination-controls button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.page-size-select {
  margin-left: 1rem;
}
</style>
