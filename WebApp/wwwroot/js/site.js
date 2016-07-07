(function worker() {
    $.get("http://localhost:8554/api/events?t=" + Math.floor((Math.random() * 1000) + 1), function (data) {
        console.log(data);

        // Recursively invoke worker function with delay
        setTimeout(worker, 1000);
    });
})();
