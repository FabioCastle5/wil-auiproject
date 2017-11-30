/* This sketch is used to make some acceleration measures over the axes
 *  without any kind of compensations but the offset and the filter
 *  provided by the MPU6050
 */

#include<Wire.h>
#include<I2Cdev.h>
#include<MPU6050.h>

/*  Offset evaluated <-> Choose optimistic or pessimistic ones
/*  [2767,2768] --> [-12,8]
 *  [-1993,-1992] --> [-7,9]
 *  [1034,1034] --> [16377,16406]
 *  [98,99] --> [-1,1]
 *  [-15,-15] --> [0,1]
 *  [46,47] --> [-1,1]/
 *  
 *  Mean AccX Error: -21.47
 *  Mean AccY Error: -5.95
 *  Mean AccX Error: 8.75
 *  Mean AccY Error: 9.68
*/

#define MPU6050_ACCEL_OFFSET_X 2767
#define MPU6050_ACCEL_OFFSET_Y -1993
#define MPU6050_ACCEL_OFFSET_Z 1034
#define MPU6050_GYRO_OFFSET_X  98
#define MPU6050_GYRO_OFFSET_Y  -15
#define MPU6050_GYRO_OFFSET_Z  46

// evaluated with acceleration_mean_error.ino sketch and mean_dispersion.ino
#define ACCX_MEAN_ERROR -21.47
#define ACCX_DISPERSION: 8.75
#define ACCY_MEAN_ERROR -5.95
#define ACCY_DISPERSION: 9.6847

#define MPU6050_DLPF_MODE 6

#define SAMPLES 100
#define INIT_SAMPLES 20
#define DELTA_T 125

// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t newaX, newaY;
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
    newaX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    newaY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    n = n + 1;
  }
  else if (n < SAMPLES + INIT_SAMPLES) {
    startingTime = millis();  // starting time instant
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers

    //gets acceleration data
    newaX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    newaY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    Wire.endTransmission(true);

    //prints results
    Serial.print("---Entry "); Serial.println(n - INIT_SAMPLES);
    Serial.print("accX = "); Serial.print(aX);
    Serial.print(" | accY = "); Serial.println(aY);

    n = n + 1;

    endingTime = millis();

    // the dalay allow to have a constant sample rate
    if (endingTime - startingTime > DELTA_T) {
      Serial.println("Warning: the sample rate is too high");
      n = SAMPLES;
    }
    else
      delay(DELTA_T - (endingTime - startingTime));
  }
  else {
    // sleep for a minute
    delay(60000);
  }
}
