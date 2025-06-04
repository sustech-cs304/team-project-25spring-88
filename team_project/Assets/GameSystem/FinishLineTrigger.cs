using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity script that detects when a car crosses the finish line in a racing game.
/// <para>
/// This script is attached to a trigger collider representing the finish line. It notifies the
/// LapCounter when a car tagged as "Player" or "AI" crosses the finish line, potentially
/// triggering race completion or lap increment logic.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现小车经过终点线，可以出发结束页面，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class FinishLineTrigger : MonoBehaviour
{
    /// <summary>
    /// Reference to the LapCounter script managing race logic.
    /// </summary>
    public LapCounter lapCounter;

    /// <summary>
    /// Handles the event when a collider enters the finish line trigger.
    /// <para>
    /// Triggers finish line logic via LapCounter for cars tagged as "Player" or "AI".
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("AI"))
        {
            if (lapCounter == null)
            {
                Debug.LogError($"[{nameof(FinishLineTrigger)}] LapCounter reference is not assigned on {gameObject.name}!");
                return;
            }
            lapCounter.OnFinishLinePassed();
            Debug.Log($"[{nameof(FinishLineTrigger)}] Finish line triggered by {other.tag} on {gameObject.name}");
        }
    }
}