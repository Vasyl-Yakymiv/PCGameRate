 document.addEventListener("DOMContentLoaded", function() {
        fetch('/Game/GetPopularGames')
            .then(response => response.json())
            .then(games => {
                let carouselContent = document.getElementById("carouselContent");
                games.forEach(game => {
                    let gameHtml = `
                        <div class="swiper-slide">
                            <div class="game-card">
                                <a href="/Game/Detail/${game.gameId}">
                                    <img src="${game.image}" alt="${game.title}" class="game-image">
                                </a>
                                <div class="game-info">
                                    <h5 class="title">${game.title}</h5>
                                    <p>⭐ ${game.ratingAverage} | 📅 ${game.releaseYear}</p>
                                </div>
                            </div>
                        </div>
                    `;
                    carouselContent.innerHTML += gameHtml;
                });

                new Swiper(".mySwiper", {
                    slidesPerView: 4,
                    spaceBetween: 20,
                    loop: true,
                    navigation: {
                        nextEl: ".swiper-button-next",
                        prevEl: ".swiper-button-prev",
                    },
                    pagination: {
                        el: ".swiper-pagination",
                        clickable: true,
                    },
                    breakpoints: {
                        320: { slidesPerView: 1 },
                        768: { slidesPerView: 2 },
                        1024: { slidesPerView: 4 }
                    }
                });
            });
    });

