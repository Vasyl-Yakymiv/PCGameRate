document.addEventListener('DOMContentLoaded', function () {
    const stars = document.querySelectorAll('.star');
    const selectedRatingInput = document.getElementById('selectedRating');

    // Наведення на зірки
    stars.forEach(star => {
        star.addEventListener('mouseover', () => {
            const value = parseInt(star.getAttribute('data-value'));
            updateStars(value);
        });

        star.addEventListener('mouseout', () => {
            const currentValue = parseInt(selectedRatingInput.value);
            updateStars(currentValue);
        });

        // Клік на зірки
        star.addEventListener('click', () => {
            const value = parseInt(star.getAttribute('data-value'));
            selectedRatingInput.value = value;
            updateStars(value);
        });
    });

    // Оновлення стану зірок
    function updateStars(rating) {
        stars.forEach(star => {
            const value = parseInt(star.getAttribute('data-value'));
            if (value <= rating) {
                star.classList.add('filled');
                star.classList.remove('hover');
            } else {
                star.classList.remove('filled');
                star.classList.add('hover');
            }
        });
    }

    // Ініціалізація стану зірок
    const initialRating = parseInt(selectedRatingInput.value) || 0;
    updateStars(initialRating);
});
