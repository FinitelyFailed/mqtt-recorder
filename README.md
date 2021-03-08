A local MQTT broker is required, such as mosquitto.
Tested and developed on Ubuntu 20.10.

Use -record to record stream from MQTT.

-url "ws://url:80/mqtt"

-topic "test/#"

-output "test" // Output file, where to save recording.

Use -send to send a recording to a local MQTT broker.

-input "test" // Input file, where to find a recodring to send.
