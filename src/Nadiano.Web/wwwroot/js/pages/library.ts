function initLibraryFilters(): void {
  const search = document.getElementById("song-search") as HTMLInputElement | null;
  const status = document.getElementById("song-status") as HTMLSelectElement | null;
  const count = document.getElementById("song-filter-count");
  const empty = document.getElementById("song-filter-empty");
  const cards = [...document.querySelectorAll<HTMLElement>("[data-song-card]")];

  if (!search || !status || !count || !empty) {
    return;
  }

  const unit = document.documentElement.lang.startsWith("id") ? "lagu" : "Stücke";

  function apply(): void {
    const query = search.value.trim().toLocaleLowerCase();
    const selectedStatus = status.value;
    let visible = 0;

    for (const card of cards) {
      const title = card.dataset.title ?? "";
      const cardStatus = card.dataset.status ?? "";
      const matchesText = query.length === 0 || title.includes(query);
      const matchesStatus = selectedStatus === "all"
        || (selectedStatus === "ready" && cardStatus === "ready")
        || (selectedStatus === "other" && cardStatus !== "ready");
      const show = matchesText && matchesStatus;
      card.hidden = !show;
      if (show) {
        visible += 1;
      }
    }

    count.textContent = `${visible} ${unit}`;
    empty.hidden = visible !== 0;
  }

  search.addEventListener("input", apply);
  status.addEventListener("change", apply);
  apply();
}

initLibraryFilters();
