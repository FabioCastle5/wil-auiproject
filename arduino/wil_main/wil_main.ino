// Main sketch for Wil toy

#include <I2Cdev.h>
#include <MPU6050.h>
// Arduino Wire library is required if I2Cdev I2CDEV_ARDUINO_WIRE implementation
// is used in I2Cdev.h
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    #include <Wire.h>
#endif
#include <ESP8266WiFi.h>
#include <WiFiClient.h>

/*  Offset evaluated <-> Choose optimistic or pessimistic ones
/*  [2919,2920] --> [-13,2]
 *  [-1951,-1950] --> [-11,3]
 *  [997,998] --> [16359,16389]
 *  [90,91] --> [-2,2]
 *  [-22,-21] --> [0,4]
 *  [55,56] --> [0,3]
*/

#define MPU6050_ACCEL_OFFSET_X 2791
#define MPU6050_ACCEL_OFFSET_Y -2083
#define MPU6050_ACCEL_OFFSET_Z 997
#define MPU6050_GYRO_OFFSET_X  89
#define MPU6050_GYRO_OFFSET_Y  -23
#define MPU6050_GYRO_OFFSET_Z  56

#define MPU6050_DLPF_MODE 6

#define ID "ESP8266-WIL"
#define PASS "WilProject"

#define INIT_SAMPLES 100
#define DELTA_T 500

// TOLERANCE defines the threshold which separates a movement from a non-movement
#define TOLERANCE_PX 1000
#define TOLERANCE_NX -1000
#define TOLERANCE_PY 1000
#define TOLERANCE_NY -1000


// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050
// wifi variables
WiFiServer server(80); //Initialize the server on Port 80
WiFiClient client;
// measures
int16_t ax, ay, az;
int aX, aY;
int moveX, moveY;
// measure counter
int n;
// time trackers
unsigned long startingTime;
unsigned long endingTime;


void setup() {
  //set the initial values for variables
  n = 0;
  // join I2C bus (I2Cdev library doesn't do this automatically)
    #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
        Wire.begin();
    #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
        Fastwire::setup(400, true);
    #endif
  mpu.initialize();
  mpu.setXAccelOffset(MPU6050_ACCEL_OFFSET_X);
  mpu.setYAccelOffset(MPU6050_ACCEL_OFFSET_Y);
  mpu.setZAccelOffset(MPU6050_ACCEL_OFFSET_Z);
  mpu.setXGyroOffset(MPU6050_GYRO_OFFSET_X);
  mpu.setYGyroOffset(MPU6050_GYRO_OFFSET_Y);
  mpu.setZGyroOffset(MPU6050_GYRO_OFFSET_Z);
  mpu.setDLPFMode((uint8_t) MPU6050_DLPF_MODE);
  #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    Wire.endTransmission(true);
  #endif
  // setup serial communication and wifi
  Serial.begin(9600);
  WiFi.mode(WIFI_AP); //ESP8266-12E is an AccessPoint
  WiFi.softAP(ID, PASS);  // Provide the (SSID, password);
  server.begin(); // Start the HTTP Server
  // read some initial values and discard them
  read_some_values();
  Serial.println("--- Server ready ---");
  Serial.print("Access Point IP: "); Serial.println(WiFi.softAPIP());
}


void loop() {
  // listen for connecting client
  client = server.available();
  if (client) {
    Serial.println("A client has connected. Serving measures...");
    // there is a client connected <-> the tablet is listening
    while (client.connected()) {
      startingTime = millis();
      // take a measure of acceleration and send data to client
      measure_and_send();
      endingTime = millis();
      if (endingTime - startingTime < DELTA_T) {
         // ensure that measures are taken with constant rate
        delay(DELTA_T - (endingTime - startingTime));
      }
      //else
        Serial.println("Warning: measure rate too high!");
    }
    Serial.println("Client disconnected");
    Serial.flush();
  }
  delay(500);
}


// useful to read and discard some raw data that may be incorrect
void read_some_values() {
  for (int i = 0; i < INIT_SAMPLES; i ++) {
    mpu.getAcceleration(&ax, &ay, &az);
  }
}


void measure_and_send() {
  mpu.getAcceleration(&ax, &ay, &az);

  aX = ax; aY = ay;
  //a movement is perceived only if it caused an acceleration over the tolerance area
  if (aX < TOLERANCE_PX && aX > TOLERANCE_NX)
    moveX = 0;
  else {
    if (aX < 0)
      moveX = -1;
    else
      moveX = 1;
  }
  if (aY < TOLERANCE_PY && aY > TOLERANCE_NY)
    moveY = 0;
  else {
    if (aY < 0)
      moveY = -1;
    else
      moveY = 1;
  }

  //send results to client - format: (-1|0|1);(0|1)\r
  client.print(moveX);
  client.print(';');
  client.print(moveY);
  client.print('\r');
  client.flush();
  Serial.print ("Sent: ");
  Serial.print(moveX);
  Serial.print(';');
  Serial.print(moveY);
  Serial.println('\r');
  Serial.flush();
}

