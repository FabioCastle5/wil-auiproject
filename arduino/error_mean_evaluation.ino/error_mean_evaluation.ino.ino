/* This sketch is useful to have a mean value for the measure error
 *  of the accelerometer - that is how much the acceleration is measured 
 *  while the accelerometer is standing
 */
 
// MPU Acceleration measurements

#include<Wire.h>
#include<I2Cdev.h>
#include<MPU6050.h>

/*  Offset evaluated <-> Choose optimistic or pessimistic ones
   AccX  [2839,2840] --> [-10,2]
   AccY  [-1985,-1984] --> [-12,13]
   AccZ  [1023,1024] --> [16364,16392]
   GyrX  [98,99] --> [-1,3]
   GyrY  [-18,-17] --> [-3,2]
   GyrZ  [47,48] --> [-2,1]
*/
#define MPU6050_ACCEL_OFFSET_X 2839
#define MPU6050_ACCEL_OFFSET_Y -1985
#define MPU6050_ACCEL_OFFSET_Z 1024
#define MPU6050_GYRO_OFFSET_X  98
#define MPU6050_GYRO_OFFSET_Y  -18
#define MPU6050_GYRO_OFFSET_Z  47

#define MPU6050_DLPF_MODE 6

#define SAMPLES 10000

// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t newaX, newaY; //,AcZ;
int n;
double avgX, avg0X;
double avgY, avg0Y;

bool finished;

void setup() {
  //initial variables
  n = 0;
  avg0X = 0;
  avg0Y = 0;
  finished = false;
  
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
  Serial.println("-----STARTING EVALUATION------");
}


void loop() {
  
  // takes tot samples of acceleration
  if (n < SAMPLES) {
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers

    n = n + 1;

    //gets acceleration data
    newaX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    newaY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    Wire.endTransmission(true);

    // evaluate mean values
    avgX = (newaX + (n - 1) * avg0X) / n;
    avgY = (newaY + (n - 1) * avg0Y) / n;

    // update the actual values for the next step
    avg0X = avgX;
    avg0Y = avgY;

    delay(100);
  }
  else {
    if (!finished) {
      // print results
      Serial.println("-----EVALUATION FINISHED-----");
      Serial.print("Mean AccX Error: "); Serial.println(avgX);
      Serial.print("Mean AccY Error: "); Serial.println(avgY);
      finished = true;
    }
    else {
      // sleep for a minute
      delay(60000);
    }
  }
}
