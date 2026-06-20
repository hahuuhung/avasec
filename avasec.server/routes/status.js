const express = require('express');
const router = express.Router();

// Memory store for device status (For demo, use DB in production)
const deviceStatusMap = new Map();

// POST /api/StatusReporting/:deviceId/status
router.post('/:deviceId/status', (req, res) => {
    const { deviceId } = req.params;
    const status = req.body;

    console.log(`Received status from ${deviceId}:`, status);

    // Store latest status
    deviceStatusMap.set(deviceId, status);

    // Broadcast to any WebSocket clients (optional, skipping for now)

    res.json({ success: true });
});

// GET /api/StatusReporting/:deviceId
router.get('/:deviceId', (req, res) => {
    const { deviceId } = req.params;
    const status = deviceStatusMap.get(deviceId);

    if (status) {
        res.json({ success: true, data: status });
    } else {
        res.status(404).json({ success: false, message: 'Status not found' });
    }
});

module.exports = router;
