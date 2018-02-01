#include <I2Cdev.h>
#include <MPU6050.h>
#include <Wire.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>

#define MPU6050_ACCEL_OFFSET_X 2791
#define MPU6050_ACCEL_OFFSET_Y -2083
#define MPU6050_ACCEL_OFFSET_Z 997
#define MPU6050_GYRO_OFFSET_X  89
#define MPU6050_GYRO_OFFSET_Y  -23
#define MPU6050_GYRO_OFFSET_Z  56

#define MPU6050_DLPF_MODE 5

#define ID "ESP8266-WIL"
#define PASS "WilProject"

#define INIT_SAMPLES 20
#define AVG_SAMPLES 30
#define SAMPLES 20
#define DELTA_T 100

// variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050
WiFiServer server(80); //Initialize the server on Port 80
WiFiClient client;
int16_t ax, ay, az;
float aX, aY;
float avgx, avgy;

int avg_samples;

void setup() {
  //set the initial values for variables
  Wire.begin();
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
  Serial.begin(9600);
  WiFi.mode(WIFI_AP); //ESP8266-12E is an AccessPoint
  WiFi.softAP(ID, PASS);  // Provide the (SSID, password);
  server.begin(); // Start the HTTP Server
  
  // read some initial values and discard them
  Serial.println("--- INITIALIZING ---");
  read_some_values();

  avgx = 0;
  avgy = 0;
  avg_samples = 0;
  
  Serial.println("--- EVALUATING OFFSET ---");
  evaluate_offset(AVG_SAMPLES);

  Serial.println("--- OFFSETS ---");
  Serial.print("Mean ax: "); Serial.println(avgx);
  Serial.print("Mean ay: "); Serial.println(avgy);
  
  Serial.println("--- Server ready ---");
  Serial.print("Access Point IP: "); Serial.println(WiFi.softAPIP());
}


// useful to read and discard some raw data that may be incorrect
void read_some_values() {
  for (int i = 0; i < INIT_SAMPLES; i ++) {
    mpu.getAcceleration(&ax, &ay, &az);
  }
}

// evaluate the initial offset to be applied to the measures
void evaluate_offset(int samples) {
  for (int i = 0; i < samples; i ++) {
    avg_samples ++;
    mpu.getAcceleration(&ax, &ay, &az);
    aX = ax/16384.;
    aY = ay/16384.;
    avgx = (aX + (avg_samples - 1) * avgx) / avg_samples;
    avgy = (aY + (avg_samples - 1) * avgy) / avg_samples;
    delay(50);
  }
}


void loop() {
  // listen for connecting client
  client = server.available();
  if (client) {
    Serial.println("A client has connected. Serving measures...");
    // there is a client connected <-> the tablet is listening
    while (client.connected()) {
      // take a measure of acceleration and send data to client
      measure_and_send();
      // waits an amount of time before sending the next measure
      delay(DELTA_T);
    }
    Serial.println("Client disconnected");
    Serial.flush();
    // read some samples and discard them
    read_some_values();
    evaluate_offset(10);
  }
  delay(500);
}


void measure_and_send() {
  mpu.getAcceleration(&ax, &ay, &az);

  aX = ax/16384. - avgx;
  aY = ay/16384. - avgy;

  //send results to client - format: (ax);(ay)\r
  client.print(aX);
  client.print(';');
  client.print(aY);
  client.print('\r');
  client.flush();
  Serial.print ("Sent: ");
  Serial.print(aX);
  Serial.print(';');
  Serial.print(aY);
  Serial.println('\r');
  Serial.flush();
}


//void loop() {
//  if (n < SAMPLES) {
//    mpu.getAcceleration(&ax, &ay, &az);
//    n ++;
//
//    aX = ax/16384. - avgx;
//    aY = ay/16384. - avgy;
//    
//    //print results
//    Serial.print ("MEASURE "); Serial.print(n); Serial.println(":");
//    Serial.print("Ax = ");Serial.print(aX);
//    Serial.print("\t\t | \t\t");
//    Serial.print("Ay = "); Serial.println(aY);
//    Serial.flush();
//  
//    delay(250);
//  }
//}


