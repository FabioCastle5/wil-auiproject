/* This sketch is used to make some acceleration measures over the axes
 *  compensations used are the offset and the filter provided by the MPU6050
 *  together with the error's mean compensation used in the final sketch
 */

#include<Wire.h>
#include<I2Cdev.h>
#include<MPU6050.h>

/*  Offset evaluated <-> Choose optimistic or pessimistic ones
/*  [2775,2776] --> [-15,7]
 *  [-1979,-1978] --> [-9,5]
 *  [999,1000] --> [16339,16396]
 *  [95,96] --> [-1,1]
 *  [-19,-19] --> [0,3]
 *  [46,47] --> [-2,1]        
 *  
 *  Mean AccX Error: -21.47
 *  Mean AccY Error: -5.95
 *  Dispersion AccX Error: 8.75
 *  Dispersion AccY Error: 9.68
*/

#define MPU6050_ACCEL_OFFSET_X 2775
#define MPU6050_ACCEL_OFFSET_Y -1979
#define MPU6050_ACCEL_OFFSET_Z 999
#define MPU6050_GYRO_OFFSET_X  95
#define MPU6050_GYRO_OFFSET_Y  -19
#define MPU6050_GYRO_OFFSET_Z  46

#define MPU6050_DLPF_MODE 6

#define SAMPLES 300
#define INIT_SAMPLES 50
#define DELTA_T 125

// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t aX, aY;
float delta_t;

int n;
// time trackers
unsigned long startingTime;
unsigned long endingTime;


void setup() {
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
  Serial.begin(9600);
  delay(1000);
  n = 0;
}


void loop() {
  // takes tot samples of acceleration
  if (n < INIT_SAMPLES) {
    // reads and skips the first lectures
    aX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    aY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    n = n + 1;
  }
  else if (n < SAMPLES + INIT_SAMPLES) {
    startingTime = millis();  // starting time instant
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers

    //gets acceleration data
    aX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    aY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    Wire.endTransmission(true);

    //prints measures
    Serial.print("---Entry "); Serial.println(n - INIT_SAMPLES);
    Serial.print("accX = "); Serial.print(aX);
    Serial.print(" | accY = "); Serial.println(aY);

    n = n + 1;

    endingTime = millis();

    // the dalay allow to have a constant sample rate
    if (endingTime - startingTime > DELTA_T) {
      Serial.println("Warning: the sample rate is too high");
      n = SAMPLES + INIT_SAMPLES;
    }
    else
      delay(DELTA_T - (endingTime - startingTime));
  }
  else {
    // sleep for a minute
    delay(60000);
  }
}
