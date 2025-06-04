using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity script that triggers the start of a race when a car crosses the start line.
/// <para>
/// This script is attached to a trigger collider representing the start line in a racing game.
/// It detects when a car tagged as "Player" or "AI" enters the trigger and notifies the LapCounter
/// to start the race logic.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现小车经过startline之后开始游戏，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class StartLineTrigger : MonoBehaviour
{
    /// <summary>
    /// Reference to the LapCounter script managing race logic.
    /// </summary>
    public LapCounter lapCounter;

    /// <summary>
    /// Handles the event when a collider enters the start line trigger.
    /// <para>
    /// Initiates race start logic via LapCounter for cars tagged as "Player" or "AI".
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("aiCar"))
        {
            if (lapCounter == null)
            {
                Debug.LogError($"[{nameof(StartLineTrigger)}] LapCounter reference is not assigned on {gameObject.name}!");
                return;
            }
            lapCounter.OnStartLinePassed();
            Debug.Log($"[{nameof(StartLineTrigger)}] Start line triggered by {other.tag} on {gameObject.name}");
        }
    }
}