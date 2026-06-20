const express = require('express');
const app = express();
const PORT = 3002;

app.get('/', (req, res) => {
    res.send('<h1>Server is running! Code is OK.</h1>');
});

app.listen(PORT, () => {
    console.log(`Simple Server running at http://localhost:${PORT}`);
});
