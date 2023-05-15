using UnityEngine;

public class PIDController {
    private float Kp; // Proportional gain
    private float Ki; // Integral gain
    private float Kd; // Derivative gain

    private float integral = 0f;
    private float prevError = 0f;

    public PIDController(float Kp, float Ki, float Kd) {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }

    public float Update(float error, float deltaTime) {
        integral += error * deltaTime;

        float derivative = (error - prevError) / deltaTime;
        prevError = error;

        float output = Kp * error + Ki * integral + Kd * derivative;
        return output;
    }

    public void updatePara(float Kp, float Ki, float Kd) {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }
}