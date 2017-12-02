// Main sketch for Wil toy

#include<Wire.h>
#include<I2Cdev.h>
#include<MPU6050.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>

/*  Offset evaluated <-> Choose optimistic or pessimistic ones
/*  [2775,2776] --> [-15,7]
 *  [-1979,-1978] --> [-9,5]
 *  [999,1000] --> [16339,16396]
 *  [95,96] --> [-1,1]
 *  [-19,-19] --> [0,3]
 *  [46,47] --> [-2,1]
*/

#define MPU6050_ACCEL_OFFSET_X 2775
#define MPU6050_ACCEL_OFFSET_Y -1979
#define MPU6050_ACCEL_OFFSET_Z 999
#define MPU6050_GYRO_OFFSET_X  95
#define MPU6050_GYRO_OFFSET_Y  -19
#define MPU6050_GYRO_OFFSET_Z  46

#define MPU6050_DLPF_MODE 6

#define ID "ESP8266-WIL"
#define PASS "WilProject"

#define INIT_SAMPLES 100
#define DELTA_T 125

// TOLERANCE defines the threshold which separates a movement from a non-movement
#define TOLERANCE_PX 200
#define TOLERANCE_NX -240
#define TOLERANCE_PY 240
#define TOLERANCE_NY -280


// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050
// wifi variables
WiFiServer server(80); //Initialize the server on Port 80
WiFiClient client;
// measures
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
  // setup the accelerometer
  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  mpu.initialize();
  mpu.setXAccelOffset(MPU6050_ACCEL_OFFSET_X);
  mpu.setYAccelOffset(MPU6050_ACCEL_OFFSET_Y);
  mpu.setZAccelOffset(MPU6050_ACCEL_OFFSET_Z);
  mpu.setXGyroOffset(MPU6050_GYRO_OFFSET_X);
  mpu.setYGyroOffset(MPU6050_GYRO_OFFSET_Y);
  mpu.setZGyroOffset(MPU6050_GYRO_OFFSET_Z);
  mpu.setDLPFMode((uint8_t) MPU6050_DLPF_MODE);
  Wire.endTransmission(true);
  // setup serial communication and wifi
  Serial.begin(115200);
  WiFi.mode(WIFI_AP); //ESP8266-12E is an AccessPoint
  WiFi.softAP(ID, PASS);  // Provide the (SSID, password); .
  server.begin(); // Start the HTTP Server
  // read some initial values and discard them
  read_some_values();
}


void loop() {
  // listen for connecting client
  client = server.available();
  if (client) {
    // there is a client connected <-> the tablet is listening
    while (client.connected()) {
      startingTime = millis();
      // take a measure of acceleration and send data to client
      measure_and_send();
      endingTime = millis();
      // ensure that measures are taken with constant rate
      delay(DELTA_T - (endingTime - startingTime));
    }
  }
}


// useful to read and discard some raw data that may be incorrect
void read_some_values() {
  for (int i = 0; i < INIT_SAMPLES; i ++) {
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers
    aX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    aY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
  }
}


void measure_and_send() {
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers

  //gets acceleration data
  aX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
  aY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
  // measure on y in inverted
  aY = -aY;

  //a movement is perceived only if it caused an acceleration over the tolerance area
  if (aX < TOLERANCE_PX && aX > TOLERANCE_NX)
    moveX = 0;
  else {
    if (aX < 0)
      moveX = -1;
    else
      moveX = 1;
  }
  if (aY < TOLERANCE_PY) // include aY < 0
    moveY = 0;
  else
    moveY = 1;

  //send results to client - format: (-1|0|1);(0|1)\r
  client.print(moveX);
  client.print(';');
  client.print(moveY);
  client.print('\r');
}
