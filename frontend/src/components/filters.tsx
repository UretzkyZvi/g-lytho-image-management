import { FC, useState } from "react";

type Filters = {
  name: string;
  size: string;
  description: string;
};

interface FiltersProps {
  filters: Filters;
  setFilters: React.Dispatch<React.SetStateAction<Filters>>;
}
export const Filters: FC<FiltersProps> = ({ filters, setFilters }) => {
  return (
    <div className="mb-4 flex flex-wrap gap-4">
      <input
        type="text"
        placeholder="Filter by Name"
        className="rounded border p-2"
        value={filters.name}
        onChange={(e) =>
          setFilters((prev) => ({ ...prev, name: e.target.value }))
        }
      />
      <input
        type="text"
        placeholder="Filter by Size"
        className="rounded border p-2"
        value={filters.size}
        onChange={(e) =>
          setFilters((prev) => ({ ...prev, size: e.target.value }))
        }
      />
      <input
        type="text"
        placeholder="Filter by Description"
        className="rounded border p-2"
        value={filters.description}
        onChange={(e) =>
          setFilters((prev) => ({
            ...prev,
            description: e.target.value,
          }))
        }
      />
      <button
        onClick={() => setFilters({ name: "", size: "", description: "" })}
        className="rounded bg-red-500 p-2 text-white"
      >
        Clear Filters
      </button>
    </div>
  );
};
