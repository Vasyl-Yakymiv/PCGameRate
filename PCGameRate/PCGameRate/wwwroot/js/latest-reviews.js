document.addEventListener("DOMContentLoaded", function () {
    fetch('/Game/GetLatestReviews')
        .then(response => response.json())
        .then(reviews => {
            console.log(reviews);

            let reviewsCarouselContent = document.getElementById("reviewsCarouselContent");
            reviewsCarouselContent.innerHTML = "";

            reviews.forEach(review => {
                let reviewHtml = `
                    <div class="swiper-slide">
                        <div class="review-card" onclick="window.location.href='/Game/Detail/${review.gameId}'">
                            <a href="/Game/Detail/${review.gameId}">
                                <img src="${review.image}" alt="${review.title}" class="game-image">
                            </a>
                            <div class="review-info">
                                <p class="username">✍ ${review.userName}</p>
                                <p class="review-text">${review.reviewText.length > 150 ? review.reviewText.substring(0, 150) + '...' : review.reviewText}</p>
                            </div>
                        </div>
                    </div>
                `;
                reviewsCarouselContent.innerHTML += reviewHtml;
            });

            new Swiper(".mySwiperReviews", {
                slidesPerView: 3,
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
                    1024: { slidesPerView: 3 }
                }
            });
        })
        .catch(error => console.error("Error fetching reviews:", error));
});
