$(document).ready(function () {
    $("#searchBox").on("input", function () {
        let query = $(this).val();
        if (query.length < 2) {
            $("#suggestions").hide();
            return;
        }

        $.ajax({
            url: "/Game/SearchSuggestions",
            type: "GET",
            data: { query: query },
            success: function (data) {
                let results = data.results;
                let suggestionsBox = $("#suggestions");
                suggestionsBox.empty();

                if (results.length === 0) {
                    suggestionsBox.hide();
                    return;
                }

                results.forEach(game => {
                    let item =`
                        <div class="suggestion-item">
                            <a href="/Game/Detail/${game.gameId}" class="game-link">
                                <img src="${game.image}" alt="${game.title}" class="game-thumb">
                                <div>
                                    <strong>${game.title}</strong>
                                    <p>⭐ ${game.ratingAverage}</p>
                                </div>
                            </a>
                        </div>
                    `;
                    suggestionsBox.append(item);
                });

                suggestionsBox.show();
            }
        });
    });

    $(document).click(function (event) {
        if (!$(event.target).closest("#searchBox, #suggestions").length) {
            $("#suggestions").hide();
        }
    });
});
