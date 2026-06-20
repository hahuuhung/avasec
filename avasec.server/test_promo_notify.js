const axios = require('axios');

async function test() {
    try {
        console.log('Sending promotional notification...');
        const response = await axios.post('http://localhost:3000/api/admin/notifications/send', {
            userId: 1,
            title: 'Summer Sale!',
            message: 'Get 50% off Premium now!',
            notificationType: 'success',
            imageUrl: 'https://example.com/sale.jpg',
            actionUrl: 'https://sysanti.com/buy',
            isPromotional: true
        });
        console.log('Send Response:', response.data);

        console.log('Fetching notifications...');
        const fetchResponse = await axios.get('http://localhost:3000/api/notifications/1');
        console.log('Fetch Response:', JSON.stringify(fetchResponse.data, null, 2));

    } catch (error) {
        console.error('Error:', error.response ? error.response.data : error.message);
    }
}

test();
