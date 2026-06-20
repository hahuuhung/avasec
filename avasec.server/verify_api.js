const http = require('http');

const options = {
    hostname: 'localhost',
    port: 3001,
    path: '/api/auth/user/1',
    method: 'GET'
};

const req = http.request(options, (res) => {
    let data = '';

    res.on('data', (chunk) => {
        data += chunk;
    });

    res.on('end', () => {
        console.log('API Response:', data);
    });
});

req.on('error', (error) => {
    console.error('Error:', error);
});

req.end();
