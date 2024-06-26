#include <ESP8266WiFi.h>
#include "Adafruit_MQTT.h"
#include "Adafruit_MQTT_Client.h"
/************************* WiFi Access Point *********************************/
#define WLAN_SSID "Fd"
#define WLAN_PASS "MarxEngelsss"
/************************* Adafruit.io Setup *********************************/
#define AIO_SERVER "io.adafruit.com"
#define AIO_SERVERPORT 1883 // use 8883 for SSL
#define AIO_USERNAME "fdgaribli"
#define AIO_KEY "aio_uqKf16NZEljV3Y94ZyZalnjiTsgi" //Adafruit AIO Key
// Create an ESP8266 WiFiClient class to connect to the MQTT server.
WiFiClient client;
Adafruit_MQTT_Client mqtt(&client, AIO_SERVER, AIO_SERVERPORT, AIO_USERNAME, AIO_KEY);
/****************************** Feeds ***************************************/
// Setup a feed called 'saumqtt' for publishing.
// Notice MQTT paths for AIO follow the form: <username>/feeds/<feedname>
Adafruit_MQTT_Publish feed = Adafruit_MQTT_Publish(&mqtt, AIO_USERNAME "/feeds/sau.mqtt");
// Setup a feed called 'onoff' for subscribing to changes.
Adafruit_MQTT_Subscribe onoff = Adafruit_MQTT_Subscribe(&mqtt, AIO_USERNAME "/feeds/sau.onoff");
/*************************** Sketch Code ************************************/
// Bug workaround for Arduino 1.6.6, it seems to need a function declaration
// for some reason (only affects ESP8266, likely an arduino‐builder bug).
void MQTT_connect();
void setup()
{
Serial.begin(115200);
delay(10);
Serial.println(F("Adafruit MQTT demo"));
// Connect to WiFi access point.
Serial.println(); Serial.println();
Serial.print("Connecting to ");
Serial.println(WLAN_SSID);
WiFi.begin(WLAN_SSID, WLAN_PASS);
while (WiFi.status() != WL_CONNECTED)
{
delay(500);
Serial.print(".");
}
Serial.println();
Serial.println("WiFi connected");
Serial.println("IP address: "); 
Serial.println(WiFi.localIP());
// Setup MQTT subscription for onoff feed.
mqtt.subscribe(&onoff);
}
uint32_t x = 0;
void loop()
{
// Ensure the connection to the MQTT server is alive (this will make the first
// connection and automatically reconnect when disconnected). See the MQTT_connect
// function definition further below.
MQTT_connect();
// this is our 'wait for incoming subscription packets' busy subloop
// try to spend your time here
Adafruit_MQTT_Subscribe* subscription;
while ((subscription = mqtt.readSubscription(5000)))
{
if (subscription == &onoff)
{
Serial.print(F("Got: "));
Serial.println((char*)onoff.lastread);
}
}
// Now we can publish stuff!
Serial.print(F("\nSending feed val "));
Serial.print(x);
Serial.print("...");
if (!feed.publish(x++))
{
Serial.println(F("Failed"));
}
else
{
Serial.println(F("OK!"));
}
// ping the server to keep the mqtt connection alive
// NOT required if you are publishing once every KEEPALIVE seconds
/*
if(! mqtt.ping()) {
mqtt.disconnect();
}
*/
}
// Function to connect and reconnect as necessary to the MQTT server.
// Should be called in the loop function and it will take care if connecting.
void MQTT_connect()
{
int8_t ret;
// Stop if already connected.
if (mqtt.connected())
{
return;
}
Serial.print("Connecting to MQTT... ");
uint8_t retries = 3;
while ((ret = mqtt.connect()) != 0)
{ // connect will return 0 for connected
Serial.println(mqtt.connectErrorString(ret));
Serial.println("Retrying MQTT connection in 5 seconds...");
mqtt.disconnect();
delay(5000); // wait 5 seconds
retries--;
if (retries == 0)
{
// basically die and wait for WDT to reset me
while (1) ;
}
}
Serial.println("MQTT Connected!");
}