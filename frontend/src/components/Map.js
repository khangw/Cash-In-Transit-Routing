import React, { useState, useCallback, useEffect } from 'react';
import MapGL, { Marker } from '@goongmaps/goong-map-react';
import Pin from './Pin';
import '../App.css';
import Papa from 'papaparse';

const GOONG_MAPTILES_KEY = 'dWK0TYbdxjUuJdllO5vrml2HUNbwjZhgi1ZRZHYr';
const GOONG_MAP_API_KEY = 'vdrLE4kfZXd3dVpB8kIuqr5ZJyoHy4We5IQs4LXP';

function Map() {
  const [viewport, setViewport] = useState({
    width: 1000,
    height: 1000,
    latitude: 21.00758683685577,
    longitude: 105.84262213793022,
    zoom: 13,
  });

  const [pins, setPins] = useState([]);
  const [solution, setSolution] = useState('');

  const handleMapClick = useCallback(
    (event) => {
      const { lngLat, srcEvent } = event;
      if (srcEvent.button === 0) {
        const newPin = {
          id: pins.length + 1,
          longitude: lngLat[0],
          latitude: lngLat[1],
        };
        setPins([...pins, newPin]);
      }
    },
    [pins]
  );

  const handleDeletePin = useCallback(
    (pinId) => {
      const updatedPins = pins.filter((pin) => pin.id !== pinId);
      setPins(updatedPins);
    },
    [pins]
  );

  const handleFileUpload = (event) => {
    const file = event.target.files[0];
    if (file) {
      Papa.parse(file, {
        header: false,
        skipEmptyLines: true,
        complete: (results) => {
          const newPins = results.data.map((row, index) => ({
            id: pins.length + index + 1,
            latitude: parseFloat(row[0]),
            longitude: parseFloat(row[1]),
          }));
          setPins([...pins, ...newPins]);
        },
      });
    }
  };

  const calculateTravelTimes = useCallback(() => {
    if (pins.length < 2) return;

    const coordinates = pins.map(pin => `${pin.latitude},${pin.longitude}`).join('|');
    const url = `https://rsapi.goong.io/DistanceMatrix?origins=${coordinates}&destinations=${coordinates}&vehicle=car&api_key=${GOONG_MAP_API_KEY}`;

    fetch(url)
      .then(response => {
        if (!response.ok) {
          throw new Error(`Error: ${response.status} - ${response.statusText}`);
        }
        return response.json();
      })
      .then(data => {
        const travelTimes = data.rows.map(row => row.elements.map(element => element.duration.value));
        console.table(travelTimes);

        // Send travelTimes to backend for solution
        fetch('https://localhost:7140/routing', {  // Ensure this matches the URL in your launchSettings.json
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(travelTimes),
        })
          .then(response => response.text())
          .then(solution => {
            console.log('Solution:', solution);
            setSolution(solution);
          })
          .catch(error => console.error('Error fetching solution:', error));
      })
      .catch(error => console.error('Error fetching travel times:', error));
  }, [pins]);

  useEffect(() => {
    calculateTravelTimes();
  }, [pins, calculateTravelTimes]);

  return (
    <React.Fragment>
      <div style={{ display: 'flex', flexDirection: 'row', height: '100vh' }}>
        <div style={{ width: '70%', position: 'relative' }}>
          <MapGL
            {...viewport}
            onViewportChange={(nextViewport) => setViewport(nextViewport)}
            goongApiAccessToken={GOONG_MAPTILES_KEY}
            onClick={handleMapClick}
          >
            {pins.map((pin) => (
              <Marker
                key={pin.id}
                longitude={pin.longitude}
                latitude={pin.latitude}
                offsetTop={-20}
                offsetLeft={-10}
              >
                <Pin size={20} />
              </Marker>
            ))}
          </MapGL>
        </div>

        <div className="PinList" style={{ width: '30%' }}>
          <h2 style={{ textAlign: 'center' }}>List Position</h2>
          <input type="file" accept=".csv" onChange={handleFileUpload} />
          {pins.map((pin) => (
            <div key={pin.id} className="PinListItem">
              <div>
                <strong>Latitude:</strong> {pin.latitude}
              </div>
              <div>
                <strong>Longitude:</strong> {pin.longitude}
              </div>
              <button onClick={() => handleDeletePin(pin.id)}>Delete</button>
            </div>
          ))}
          {solution && (
            <div>
              <h3>Solution:</h3>
              <pre>{solution}</pre>
            </div>
          )}
        </div>
      </div>
    </React.Fragment>
  );
}

export default Map;
