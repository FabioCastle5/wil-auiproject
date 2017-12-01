// Main sketch for Wil toy

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
 *  Dispersion AccX Error: 8.75
 *  Dispersion AccY Error: 9.68
*/

#define MPU6050_ACCEL_OFFSET_X 2767
#define MPU6050_ACCEL_OFFSET_Y -1993
#define MPU6050_ACCEL_OFFSET_Z 1034
#define MPU6050_GYRO_OFFSET_X  98
#define MPU6050_GYRO_OFFSET_Y  -15
#define MPU6050_GYRO_OFFSET_Z  46

// evaluated with acceleration_mean_error.ino sketch and mean_dispersion.ino
#define ACCX_MEAN_ERROR -21.47
#define ACCY_MEAN_ERROR -5.95

#define MPU6050_DLPF_MODE 6

#define SAMPLES 100
#define INIT_SAMPLES 20
#define DELTA_T 125

// TOLERANCE defines the threshold which separates a movement from a non-movement
#define TOLERANCE_PX 50
#define TOLERANCE_NX -50
#define TOLERANCE_PY 50
#define TOLERANCE_NY -50


// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t newaX, newaY;
int aX, aY;
int moveX, moveY;
// measure counter
int n;
// time trackers
unsigned long startingTime;
unsigned long endingTime;
// communication handlers
bool active;

void setup() {
  //set the initial values for variables
  n = 0;
  active = true;  // it should be enabled by the tablet instead
  
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
}


bool tolerable_x(int16_t input) {
  if (input < TOLERANCE_PX && input > TOLERANCE_NX)
    return true;
  else
    return false;
}

bool tolerable_y(int16_t input) {
  if (input < TOLERANCE_PY && input > TOLERANCE_NY)
    return true;
  else
    return false;
}

int correct_accX(int16_t ax) {
  int corrAx;
  corrAx = ax - ACCX_MEAN_ERROR;
  return corrAx;
}

int correct_accY(int16_t ay) {
  int corrAy;
  corrAy = ay - ACCY_MEAN_ERROR;
  return - corrAy;
}


void loop() {
  if (active) {
    measureAndCommunicate();
  }
  else {
    // understand if tablet is calling it
    delay (60000);
  }

void measureAndCommunicate() {
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

    //corrects raw data
    aX = correct_accX(newaX);
    aY = correct_accY(newaY);

    //a movement is perceived only if it caused an acceleration over the tolerance area
    if (tolerable_x(aX))
      moveX = 0;
    else
      moveX = 1;
    if (tolerable_y(aY))
      moveY = 0;
    else
      moveY = 1;

    //prints results
    Serial.print("---Entry "); Serial.println(n - INIT_SAMPLES + 1);
    Serial.print("accX = "); Serial.print(aX);
    Serial.print(" | accY = "); Serial.println(aY);
    Serial.print("moveX = "); Serial.print(moveX);
    Serial.print(" | moveY = "); Serial.println(moveY);

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
    active = false;
  }
}
