import fs from 'fs';
import http from 'http';

async function validateMigration() {
  const roomsData = JSON.parse(fs.readFileSync('src/data/rooms.json', 'utf8'));
  
  return new Promise((resolve, reject) => {
    http.get('http://localhost:5088/api/properties', (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try {
          const dbData = JSON.parse(data);
          
          console.log('\n=== Migration Validation Report ===\n');
          console.log(`Source (rooms.json):    ${roomsData.length} properties`);
          console.log(`Database:               ${dbData.length} properties`);
          console.log(`Match:                  ${roomsData.length === dbData.length ? '✓ YES' : '✗ NO'}`);
          
          if (roomsData.length === dbData.length && dbData.length > 0) {
            console.log('\nSpot Check (First Property):');
            const sourceRoom = roomsData[0];
            const dbRoom = dbData[0];
            console.log(`  Name:     ${sourceRoom.name === dbRoom.name ? '✓' : '✗'} ${dbRoom.name}`);
            console.log(`  Area:     ${sourceRoom.area === dbRoom.area ? '✓' : '✗'} ${dbRoom.area}`);
            console.log(`  Rent:     ${sourceRoom.monthlyRent === dbRoom.monthlyRent ? '✓' : '✗'} ${dbRoom.monthlyRent}`);
            console.log(`  Features: ${dbRoom.features && dbRoom.features.length > 0 ? '✓' : '✗'} ${dbRoom.features?.length || 0} items`);
            console.log(`  Images:   ${dbRoom.images && dbRoom.images.length > 0 ? '✓' : '✗'} ${dbRoom.images?.length || 0} items`);
            console.log('\n✓ Migration validated successfully!\n');
          }
          
          resolve();
        } catch (error) {
          reject(new Error(`Failed to parse API response: ${error.message}`));
        }
      });
    }).on('error', reject);
  });
}

validateMigration()
  .then(() => process.exit(0))
  .catch(err => {
    console.error(`✗ Validation failed: ${err.message}`);
    process.exit(1);
  });
