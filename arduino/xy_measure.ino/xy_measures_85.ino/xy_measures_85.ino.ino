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

// evaluated with acceleration_mean_error.ino sketch
#define ACCX_MEAN_ERROR 727.01
#define ACCX_DISPERSION: 7.14
#define ACCY_MEAN_ERROR -10.16
#define ACCY_DISPERSION: 13.08

#define MPU6050_DLPF_MODE 6

#define SAMPLES 100
#define INIT_SAMPLES 20
#define DELTA_T 125
#define UNIT 0.000061035
#define BETA 0.80
#define G 5

#define TOLERANCE 50


// hw variables
MPU6050 mpu;
const int MPU_addr = 0x68; // I2C address of the MPU-6050

// measures
int16_t newaX, newaY; //,AcZ;
int aoX, aoY; //,prevZ;
int16_t deltaX, deltaY; //,deltaZ;
int aX, aY;
float vX, vY;
float voX, voY;
float x, y;
float resX, resY;
float xo, yo;
float delta_t;

int n;
// time trackers
unsigned long startingTime;
unsigned long endingTime;


void setup() {
  //initial variables
  aoX = 0;
  aoY = 0;
  //prevZ = 0;
  deltaX = 0;
  deltaY = 0;
  //deltaZ = 0;
  voX = 0.0;
  voY = 0.0;
  xo = 0.0;
  yo = 0.0;
  n = 0;
  //delta_t = DELTA_T / 1000; // in seconds
  
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


bool tolerable(int16_t input) {
  if (input < TOLERANCE && input > -TOLERANCE)
    return true;
  else
    return false;
}

int correct_accX(int16_t ax) {
  int corrAx;
  if (ax > 0)
    corrAx = ax - ACCX_MEAN_ERROR;
  else
    if (ax < 0)
      corrAx = ax + ACCX_MEAN_ERROR;
    else  //ax = 0
      corrAx = ax;
  return corrAx;
}

int correct_accY(int16_t ay) {
  int corrAy;
  if (ay > 0)
    corrAy = ay + ACCY_MEAN_ERROR;
  else
    if (ay < 0)
      corrAy = ay - ACCY_MEAN_ERROR;
    else  //ay = 0
      corrAy = ay;
  return corrAy;
}


void loop() {
  // takes tot samples of acceleration
  if (n < INIT_SAMPLES) {
    n = n + 1;
    // reads and skips the first lectures
    newaX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    newaY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
  }
  else if (n < SAMPLES + INIT_SAMPLES) {
    startingTime = millis();  // starting time instant
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers

    n = n + 1;

    //gets acceleration data
    newaX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    newaY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    //AcZ=Wire.read()<<8|Wire.read();  // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)
    Wire.endTransmission(true);

    //get the delta between the current value and the previous one
    deltaX = newaX - aoX;
    deltaY = newaY - aoY;
    //deltaZ = AcZ - prevZ;

    //the value is a useful value if it goes outside the tolerance area
    if (tolerable(deltaX))
      aX = 0;
    else
      aX = correct_accX(newaX);
    if (tolerable(deltaY))
      aY = 0;
    else
      aY = correct_accY(newaY);

    // the first sample is a starting offset, which has to be discarded
    if (n > 1) {
    // evaluate speed and position
    vX = aX * DELTA_T * UNIT * G + voX * (1 - BETA);
    vY = aY * DELTA_T * UNIT * G + voY * (1 - BETA);
    x = vX + xo;
    y = vY + yo;

    //prints results
      Serial.print("---Entry "); Serial.println(n - INIT_SAMPLES);
      Serial.print("accX = "); Serial.print(aX);
      Serial.print(" | accY = "); Serial.println(aY);
      Serial.print("velX = "); Serial.print(vX);
      Serial.print(" | velY = "); Serial.println(vY);
      Serial.print("x = "); Serial.print(x);
      Serial.print(" | y = "); Serial.println(y);
    }
    else {
      // the first round must be invisible for the evaluation of speed and position
      vX = 0.0;
      vY = 0.0;
      x = 0.0;
      y = 0.0;
    }

    // update the actual values for the next step
    aoX = newaX;
    aoY = newaY;
    //prevZ = AcZ;
    voX = vX;
    voY = vY;
    xo = x;
    yo = y;

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
