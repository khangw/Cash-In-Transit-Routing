import React, { useState, useCallback, useEffect } from 'react';
import MapGL, { Marker, Source, Layer } from '@goongmaps/goong-map-react';
import ExpandMoreOutlinedIcon from '@mui/icons-material/ExpandMoreOutlined';
import ExpandLessOutlinedIcon from '@mui/icons-material/ExpandLessOutlined';
import DeleteForeverOutlinedIcon from '@mui/icons-material/DeleteForeverOutlined';
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined';
import LocationOnOutlinedIcon from '@mui/icons-material/LocationOnOutlined';
import Pin from './Pin';
import '../map.css';
import Papa from 'papaparse';
import axios from "axios";

const GOONG_MAPTILES_KEY = 'dWK0TYbdxjUuJdllO5vrml2HUNbwjZhgi1ZRZHYr';
const GOONG_MAP_API_KEY = 'vdrLE4kfZXd3dVpB8kIuqr5ZJyoHy4We5IQs4LXP';

const colors = ['#FF0000', '#00FF00', '#0000FF', '#FFFF00', '#FF00FF', '#00FFFF'];

function Map() {
  const [viewport, setViewport] = useState({
    width: 1460,
    height: 1000,
    latitude: 21.00758683685577,
    longitude: 105.84262213793022,
    zoom: 13,
  });

  const [autoDrawRoute, setAutoDrawRoute] = useState(false);
  const [routes, setRoutes] = useState([]);
  const [pins, setPins] = useState([]);
  const [solution, setSolution] = useState(null);
  const [expand, setExpand] = useState(true) // state đóng mở list
  const [homeStates, setHomeStates] = useState({}); // state chọn điểm quay về

  const handleHomeClick = (pinId) => {
    setHomeStates(prevStates => ({
      ...Object.keys(prevStates).reduce((acc, key) => {
        acc[key] = false;
        return acc;
      }, {}),
      [pinId]: true
    }));
  };


  const handleMapClick = useCallback(
    (event) => {
      const { lngLat, srcEvent } = event;
      if (srcEvent.button === 0) {
        const newPin = {
          id: pins.length + 1,
          longitude: lngLat[0],
          latitude: lngLat[1],
          name: '',
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
            name: '',
          }));
          setPins([...pins, ...newPins]);
        },
      });
    }
  };

  const handleNameChange = (id, name) => {
    const updatedPins = pins.map(pin => (pin.id === id ? { ...pin, name } : pin));
    setPins(updatedPins);
  };

  const handleAutoDrawRoute = useCallback(() => {
    setAutoDrawRoute(true);
  }, []);

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

        fetch('https://localhost:7140/routing', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(travelTimes),
        })
          .then(response => response.text())
          .then(solution => {
            setSolution(solution);
          })
          .catch(error => console.error('Error fetching solution:', error));
      })
      .catch(error => console.error('Error fetching travel times:', error));
  }, [pins]);

  useEffect(() => {
    calculateTravelTimes();
  }, [pins, calculateTravelTimes]);

  function GetRouteArrayByIndex(index, solution) {
    const subArrayStrings = solution.slice(1, -1).split('],[');
    const mainArray = subArrayStrings.map(subArrayString => {
      return subArrayString.replace('[', '').replace(']', '').split(',').map(Number);
    });

    if (index >= 0 && index < mainArray.length) {
      return GetPinsArray(mainArray[index]);
    } else {
      return null;
    }
  }

  function GetPinsArray(array) {
    var objArr = [];
    for (let i = 0; i < array.length; i++) {
      objArr[i] = pins[array[i]];
    }
    return objArr;
  }

  function CountRoute(solution) {
    const subArrayStrings = solution.slice(1, -1).split('],[');
    return subArrayStrings.length;
  }

  const direct = (pins, color) => {
    if (autoDrawRoute && pins.length > 1) {
      const fetchDirections = async () => {
        let combinedRoute = [];
        for (let i = 0; i < pins.length - 1; i++) {
          const origin = pins[i];
          const destination = pins[i + 1];

          const url = `https://rsapi.goong.io/Direction?origin=${origin.latitude},${origin.longitude}&destination=${destination.latitude},${destination.longitude}&vehicle=car&api_key=${GOONG_MAP_API_KEY}`;

          try {
            const response = await axios.get(url);
            if (response.data.routes.length > 0) {
              const routeData = response.data.routes[0].overview_polyline.points;
              const decodedRoute = decodePolyline(routeData);
              combinedRoute = combinedRoute.concat(decodedRoute);
            }
          } catch (error) {
            console.error('Error fetching directions:', error);
          }
        }
        setRoutes(prevRoutes => [...prevRoutes, { route: encodePolyline(combinedRoute), color }]);
      };

      fetchDirections();
      setAutoDrawRoute(false);
    }
  }

  useEffect(() => {
    if (solution != null) {
      var vehicles = CountRoute(solution);
      for (let i = 0; i < vehicles; i++) {
        console.log(GetRouteArrayByIndex(i, solution));
        direct(GetRouteArrayByIndex(i, solution), colors[i % colors.length]);
      }
    }

  }, [autoDrawRoute, pins, solution]);

  const encodePolyline = (points) => {
    let encoded = '';
    let prevLat = 0;
    let prevLng = 0;

    for (let i = 0; i < points.length; ++i) {
      const lat = points[i].latitude;
      const lng = points[i].longitude;
      const encodedLat = encodeCoordinate(lat - prevLat);
      const encodedLng = encodeCoordinate(lng - prevLng);

      prevLat = lat;
      prevLng = lng;

      encoded += encodedLat + encodedLng;
    }

    return encoded;
  };

  const encodeCoordinate = (coordinate) => {
    coordinate = Math.round(coordinate * 1e5);
    coordinate <<= 1;
    if (coordinate < 0) {
      coordinate = ~coordinate;
    }
    let output = '';
    while (coordinate >= 0x20) {
      output += String.fromCharCode((0x20 | (coordinate & 0x1f)) + 63);
      coordinate >>= 5;
    }
    output += String.fromCharCode(coordinate + 63);
    return output;
  };

  const decodePolyline = (polyline) => {
    let points = [];
    let index = 0, len = polyline.length;
    let lat = 0, lng = 0;

    while (index < len) {
      let b, shift = 0, result = 0;
      do {
        b = polyline.charCodeAt(index++) - 63;
        result |= (b & 0x1f) << shift;
        shift += 5;
      } while (b >= 0x20);
      let dlat = ((result & 1) ? ~(result >> 1) : (result >> 1));
      lat += dlat;

      shift = 0;
      result = 0;
      do {
        b = polyline.charCodeAt(index++) - 63;
        result |= (b & 0x1f) << shift;
        shift += 5;
      } while (b >= 0x20);
      let dlng = ((result & 1) ? ~(result >> 1) : (result >> 1));
      lng += dlng;

      points.push({ latitude: (lat / 1E5), longitude: (lng / 1E5) });
    }
    return points;
  };

  return (
    <React.Fragment>
      <div className='wrapper'>
        <div className='map'>
          <MapGL
            className="map-container"
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
                <Pin size={20} name={pin.name} />
              </Marker>
            ))}
            {routes.map((route, index) => {
              const routeCoordinates = route.route ? decodePolyline(route.route).map(point => [point.longitude, point.latitude]) : [];
              return (
                <Source key={index} id={`route-${index}`} type="geojson" data={{
                  type: 'Feature',
                  geometry: {
                    type: 'LineString',
                    coordinates: routeCoordinates
                  }
                }}>
                  <Layer
                    id={`route-layer-${index}`}
                    type="line"
                    source={`route-${index}`}
                    layout={{
                      'line-join': 'round',
                      'line-cap': 'round'
                    }}
                    paint={{
                      'line-color': route.color,
                      'line-width': 8
                    }}
                  />
                </Source>
              );
            })}
          </MapGL>
        </div>

        <div className='wrapper-pin-list'>
          <div className='header-pin'>
            <span>Viet Nam</span>
            <span>Gần đây</span>
          </div>
          <hr />

          <div className='file-upload'>
            <span>Upload file</span>
            <input type="file" accept=".csv" onChange={handleFileUpload} />
          </div>
          <hr />

          {
            expand
              ?
              <div className='expand' onClick={() => setExpand(false)}>
                <ExpandLessOutlinedIcon className='icon-expand' />
                <span>Thu gọn</span>
              </div>
              :
              <div className='expand' onClick={() => setExpand(true)}>
                <ExpandMoreOutlinedIcon className='icon-expand' />
                <span>Xem danh sách tại đây</span>
              </div>
          }

          <div style={{ display: expand ? 'block' : 'none' }} className="pin-list">
            {pins.map((pin, index) => (
              <div key={pin.id} className="pin-item">
                <div className="pin-item-left">
                  <div className="top-pin-left">
                    <span>Phòng giao dịch số {index + 1}</span>
                    <input type="text" placeholder='Tên phòng giao dịch' />
                  </div>
                  <div className="bottom-pin-left">
                    <DeleteForeverOutlinedIcon  onClick={() => {handleDeletePin(pin.id)}} className='remove'/>
                    <HomeOutlinedIcon
                      onClick={() => handleHomeClick(pin.id)} 
                      className={`home ${homeStates[pin.id] ? 'active' : ''}`}
                    />
                  </div>
                </div>

                <div className="pin-item-right">
                  <div><LocationOnOutlinedIcon className='icon'/> <span>{pin.latitude}</span></div>
                  <div><LocationOnOutlinedIcon className='icon'/> <span>{pin.longitude}</span></div>
                </div>
                
              </div>
            ))}
            {solution && (
              <div>
                <h3>Solution:</h3>
                <pre>{solution}</pre>
                <button onClick={handleAutoDrawRoute}>Draw Routes</button>
              </div>
            )}
          </div>
        </div>
        <div className='time-max'>
            <input  type="text" name="" id="datetime" placeholder='Time max' />
            <span>Đặt</span>
        </div>

      </div>
    </React.Fragment>
  );
}

export default Map;
