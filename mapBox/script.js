mapboxgl.accessToken = 'pk.eyJ1IjoiamVzc2Vib2x0b24iLCJhIjoiY2p4OXoybzIwMHJzMjN5bnJ1eWwyOXpwcSJ9.4OuFCNnUNOZodRjj9NhmHw';

var map = new mapboxgl.Map({
    container: 'map', // container id
    style: 'mapbox://styles/jessebolton/cjx9zmf0l35gy1cps8cqind3h' // replace this with your style URL
  });




map.on('load', function(){

    map.getCanvas().style.cursor = 'default';

    map.fitBounds([[-133.2421875, 16.972741], [-47.63671875, 52.696361]]);

    var layers = ['0-10', '10-20', '20-50', '50-100', '100-200', '200-500', '500-1000', '1000+'];
    var colors = ['#FFEDA0', '#FED976', '#FEB24C', '#FD8D3C', '#FC4E2A', '#E31A1C', '#BD0026', '#800026'];
    console.log("fdssdfsdfsdf")

    for (i = 0; i < layers.length; i++) {
        var layer = layers[i];
        var color = colors[i];
        var item = document.createElement('div');
        var key = document.createElement('span');
        key.className = 'legend-key';
        key.style.backgroundColor = color;
      
        var value = document.createElement('span');
        value.innerHTML = layer;
        item.appendChild(key);
        item.appendChild(value);
        legend.appendChild(item);
      }

    map.on('mousemove', function(e) {
    var states = map.queryRenderedFeatures(e.point, {
        layers: ['statedata']
    });
    
    if (states.length > 0) {
        document.getElementById('pd').innerHTML = '<h3><strong>' + states[0].properties.name + '</strong></h3><p><strong><em>' + states[0].properties.density + '</strong> people per square mile</em></p>';
    } else {
        document.getElementById('pd').innerHTML = '<p>Hover over a state!</p>';
    }
    });    
    

});

