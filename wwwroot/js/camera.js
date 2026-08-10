window.sabCamera = {
    stream: null,

    async start(videoId) {
        try {
            const video = document.getElementById(videoId);
            if (!video) return;

            this.stream = await navigator.mediaDevices.getUserMedia({
                video: { width: 1280, height: 720 }
            });

            video.srcObject = this.stream;
            await video.play();
        } catch (err) {
            console.error("Error starting camera:", err);
        }
    },

    stop() {
        if (!this.stream) return;
        this.stream.getTracks().forEach(t => t.stop());
        this.stream = null;
    },

    capture(videoId) {
        const video = document.getElementById(videoId);
        if (!video) return "";

        const canvas = document.createElement("canvas");
        canvas.width = 1024;
        canvas.height = 800;

        const ctx = canvas.getContext("2d");
        // Drawn using full canvas width and height
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        const data = canvas.toDataURL("image/jpeg", 0.9);
        return data;
    },
    async scanBarcode(videoId) {
        const video = document.getElementById(videoId);
        if (!video) return "";

        if ('BarcodeDetector' in window) {
            try {
                const barcodeDetector = new BarcodeDetector();
                const barcodes = await barcodeDetector.detect(video);
                if (barcodes.length > 0) {
                    return barcodes[0].rawValue;
                }
            } catch (err) {
                console.error("Barcode detection error:", err);
            }
        } else {
            console.warn("BarcodeDetector API is not supported in this browser.");
        }
        return "";
    },
    // Capture image and send it to the backend controller
    async captureAndSave(videoId) {
        const imageData = this.capture(videoId);
        if (!imageData) return null;

        try {
            const response = await fetch('/api/upload/save', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ imageData: imageData })
            });
            return await response.json();
        } catch (err) {
            console.error("Failed to upload image:", err);
            return { success: false };
        }
    },

    // Fetch saved image relative paths from server storage
    async getSavedImages(dateString) {
        try {
            const url = dateString
                ? `/api/upload/images?date=${dateString}`
                : '/api/upload/images';
            const response = await fetch(url);
            const data = await response.json();
            return data.images || [];
        } catch (err) {
            console.error("Failed to fetch images:", err);
            return [];
        }
    }
};