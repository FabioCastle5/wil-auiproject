/*
 * This sketch can be used to evaluate the acceleration that cause
 * some kind of movement
 */

#include<Wire.h>
#include<I2Cdev.h>
#include<MPU6050.h>

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

#define INIT_SAMPLES 20
#define SAMPLES 50
#define DELTA_T 250

// TOLERANCE defines the threshold which separates a movement from a non-movement
#define TOLERANCE_PX 1000
#define TOLERANCE_NX -1000
#define TOLERANCE_PY 1000
#define TOLERANCE_NY -1000


// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t aX, aY;
int moveX, moveY;
int n;
// time trackers
unsigned long startingTime;
unsigned long endingTime;


void setup() {
  n = 0;
  
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

//this function is the core of the sketch:
// whenever the input given is not tolerable, the program stops and you can
// read the value of acceleration that has not been tolered
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


void loop() {
  // takes tot samples of acceleration
  if (n < INIT_SAMPLES) {
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 4, true); // request a total of 14 registers
    // reads and skips the first lectures
    aX = Wire.read() << 8; // 0x3B (ACCEL_XOUT_H)
    aX |= Wire.read(); // 0x3C (ACCEL_XOUT_L)
    aY = Wire.read() << 8; // 0x3D (ACCEL_YOUT_H)
    aY |= Wire.read(); // 0x3E (ACCEL_YOUT_L)
    n = n + 1;
  }
  else if (n < INIT_SAMPLES + SAMPLES) {
    if (n == INIT_SAMPLES){
      Serial.println("--- program ready ---");
    }
    startingTime = millis();  // starting time instant
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 4, true); // request a total of 14 registers

    //gets acceleration data
    aX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    aY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    // measure on y in inverted
    aY = -aY;

    //if (!tolerable_x(aX)) {
    //  //prints measure
    //  Serial.print("Acceleration measured: ");
    //  Serial.print("accX = "); Serial.println(aX);
    //}

    //if (!tolerable_y(aY)) {
    //  //prints measure
    //  Serial.print("Acceleration measured: ");
    //  Serial.print("accY = "); Serial.println(aY);
    //  finished = true;
    //}

    //a movement is perceived only if it caused an acceleration over the tolerance area
    if (tolerable_x(aX))
      moveX = 0;
    else {
      if (aX < 0)
        moveX = -1;
      else
        moveX = 1;
    }
    if (tolerable_y(aY))
      moveY = 0;
    else {
      if (aY < 0)
        moveY = 0;
      else
        moveY = 1;
    }

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
    }
    else
      delay(DELTA_T - (endingTime - startingTime));
  }
  else {
    // sleep for 10 seconds
    delay(5000);
    // I read some character -> restart
    if (Serial.available() > 0) {
      n = 0;
      while (Serial.available() > 0)
        Serial.read();
      Serial.println("--- restarting ---");
    }
    delay(5000);
  }
}
