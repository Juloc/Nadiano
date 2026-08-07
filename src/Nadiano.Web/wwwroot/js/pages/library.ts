function initLibraryFilters(): void {
  const search = document.getElementById("song-search") as HTMLInputElement | null;
  const source = document.getElementById("song-source") as HTMLSelectElement | null;
  const status = document.getElementById("song-status") as HTMLSelectElement | null;
  const count = document.getElementById("song-filter-count");
  const empty = document.getElementById("song-filter-empty");
  const cards = [...document.querySelectorAll<HTMLElement>("[data-song-card]")];

  if (!search || !source || !status || !count || !empty) {
    return;
  }

  const searchInput = search;
  const sourceSelect = source;
  const statusSelect = status;
  const countLabel = count;
  const emptyState = empty;
  const unit = document.documentElement.lang.startsWith("id") ? "lagu" : "Stücke";

  function apply(): void {
    const query = searchInput.value.trim().toLocaleLowerCase();
    const selectedSource = sourceSelect.value;
    const selectedStatus = statusSelect.value;
    let visible = 0;

    for (const card of cards) {
      const searchText = card.dataset.title ?? "";
      const cardSource = card.dataset.source ?? "";
      const cardStatus = card.dataset.status ?? "";
      const matchesText = query.length === 0 || searchText.includes(query);
      const matchesSource = selectedSource === "all" || selectedSource === cardSource;
      const matchesStatus = selectedStatus === "all"
        || (selectedStatus === "ready" && cardStatus === "ready")
        || (selectedStatus === "other" && cardStatus !== "ready");
      const show = matchesText && matchesSource && matchesStatus;
      card.hidden = !show;
      if (show) {
        visible += 1;
      }
    }

    countLabel.textContent = `${visible} ${unit}`;
    emptyState.hidden = visible !== 0;
  }

  searchInput.addEventListener("input", apply);
  sourceSelect.addEventListener("change", apply);
  statusSelect.addEventListener("change", apply);
  apply();
}

initLibraryFilters();
