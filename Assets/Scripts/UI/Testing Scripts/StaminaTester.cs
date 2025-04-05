using UnityEngine;

public class StaminaSystemSimulator : MonoBehaviour
{
    [Header("Stamina Logic Simulation")]
    public float maxStamina = 100f;
    [SerializeField] // Keep track visually in inspector
    private float currentStamina;
    public float staminaDrainRate = 15f; // Stamina units drained per second while sprinting key is held
    public float staminaRegenRate = 8f;  // Stamina units regenerated per second
    public float regenDelay = 2.0f;    // Delay in seconds after stopping stamina use before regen starts

    [Header("Simulation Controls")]
    public float jumpStaminaCost = 25f;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode continuousDrainKey = KeyCode.LeftShift;

    // Internal Simulation State
    private float timeSinceLastDrain = 0f;
    private bool isDrainingContinuously = false;

    void Start()
    {
        currentStamina = maxStamina;
        timeSinceLastDrain = regenDelay; // Allow immediate regen if needed (though starting full)

        GameManager.UIManager.staminaBarUI.UpdateStamina(currentStamina, maxStamina);
        Debug.Log($"StaminaSystemSimulator: Ready. Press '{jumpKey}' to jump ({jumpStaminaCost} cost). Hold '{continuousDrainKey}' to drain.");
    }

    void Update()
    {
        float previousStamina = currentStamina; // Store value before changes this frame
        isDrainingContinuously = false; // Reset flag each frame

        // --- Simulate Input and Stamina Drain ---
        // Jump (Instantaneous)
        if (Input.GetKeyDown(jumpKey))
        {
            if (currentStamina >= jumpStaminaCost)
            {
                currentStamina -= jumpStaminaCost;
                Debug.Log($"Simulator: Jumped! Cost: {jumpStaminaCost}, Now: {currentStamina:F1}");
                timeSinceLastDrain = 0f; // Reset regen delay
            }
            else
            {
                Debug.Log($"Simulator: Cannot jump. Need: {jumpStaminaCost}, Have: {currentStamina:F1}");
            }
        }

        if (Input.GetKey(continuousDrainKey))
        {
            float drainAmount = staminaDrainRate * Time.deltaTime;
            if (currentStamina > 0) // Only drain if there's stamina left
            {
                isDrainingContinuously = true;
                currentStamina -= drainAmount;
                timeSinceLastDrain = 0f; // Reset regen delay while draining
            }
        }

        // Clamp stamina after drains
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);


        // --- Simulate Regeneration ---
        if (!isDrainingContinuously && currentStamina < maxStamina)
        {
            // Only increment timer or regenerate if not currently draining
            if (timeSinceLastDrain >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); // Clamp after regen
            }
            else
            {
                timeSinceLastDrain += Time.deltaTime;
            }
        }

        if (currentStamina != previousStamina) // Optimization: Only call if value changed
        {
            GameManager.UIManager.staminaBarUI.UpdateStamina(currentStamina, maxStamina);
        }
    }
}