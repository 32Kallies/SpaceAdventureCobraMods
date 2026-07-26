using System.Collections.Generic;
using UnityEngine;

namespace PscyhogunArmOverhaul;

public class PsychogunStateRememberer : MonoBehaviour
{
    private static PsychogunStateRememberer Instance { get; set; }
    
    private readonly Dictionary<Token.HardCodedTokens, bool> _tokens = new();

    private void Awake()
    {
        Instance = this;
    }

    public static PsychogunStateRememberer GetInstance(bool canBeNull = false)
    {
        if (!canBeNull && Instance == null)
        {
            Plugin.Logger.LogWarning("PsychogunStateRememberer was missing");
            Instance = new GameObject("PsychogunStateRememberer").AddComponent<PsychogunStateRememberer>();
        }
        return Instance;
    }
    
    public void SetToken(Token.HardCodedTokens token, bool active)
    {
        bool wasActive = _tokens.TryGetValue(token, out var storedTokenValue) && storedTokenValue;
        if (wasActive && !active)
        {
            TokenController.SetTokenValue(token, 1, Token.ValueOperator.Minus);
            _tokens[token] = false;
        }
        else if (!wasActive && active)
        {
            TokenController.SetTokenValue(token, 1, Token.ValueOperator.Add);
            _tokens[token] = true;
        }
    }
}