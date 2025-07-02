window.renderWeeklyChart = function (labels, data) {
    const ctx = document.getElementById('weeklyChart').getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo'],
            datasets: [{
                label: 'Publicaciones por día',
                data: data,
                backgroundColor: 'rgba(0, 123, 255, 0.6)',
                borderColor: 'rgba(0, 123, 255, 1)',
                borderWidth: 1,
                borderRadius: 8,
                hoverBackgroundColor: 'rgba(0, 123, 255, 0.9)',
                hoverBorderColor: '#000'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    labels: {
                        font: {
                            size: 14,
                            family: 'Segoe UI'
                        },
                        color: '#333'
                    }
                },
                tooltip: {
                    backgroundColor: '#fefefe',
                    titleColor: '#000',
                    bodyColor: '#000',
                    borderColor: '#ddd',
                    borderWidth: 1
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0,
                        color: '#666',
                        font: {
                            size: 13
                        }
                    },
                    grid: {
                        color: '#eee'
                    }
                },
                x: {
                    ticks: {
                        color: '#666',
                        font: {
                            size: 13
                        }
                    },
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}
