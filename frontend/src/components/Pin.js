//Pin.js

import * as React from 'react';

const ICON = `M10 20S3 10.87 3 7a7 7 0 1114 0c0 3.87-7 13-7 13zm0-11a2 2 0 100-4 2 2 0 000 4z`;

const pinStyle = {
  fill: '#d00',
  stroke: 'none'
};

function Pin(props) {
  const {size = 20} = props;

  return (
    <svg height={size} viewBox="0 0 20 20" style={pinStyle}>
      <path d={ICON} />
    </svg>
  );
}

export default React.memo(Pin);