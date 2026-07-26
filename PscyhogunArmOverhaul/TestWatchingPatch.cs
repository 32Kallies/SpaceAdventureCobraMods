/*
using System;
using HarmonyLib;
using UnityEngine;

namespace PscyhogunArmOverhaul;

[HarmonyPatch(typeof(TokenController), nameof(TokenController.SetTokenValue))]
internal static class SetTokenValuePatches
{
    [HarmonyPostfix]
    [HarmonyPatch([
        typeof(Token.TokenType),
        typeof(int),
        typeof(int),
        typeof(Token.ValueOperator),
        typeof(GameObject)
    ])]
    private static void SetTokenValue_Postfix(
        Token.TokenType type, int id)
    {
        if (type != Token.TokenType.HardCoded) return;
        if (!IsValid((Token.HardCodedTokens)id))
            return;
        Plugin.Logger.LogInfo("Token type: " + (Token.HardCodedTokens)id);
        LogStackTrace();
    }

    [HarmonyPostfix]
    [HarmonyPatch([
        typeof(Token),
        typeof(int),
        typeof(Token.ValueOperator),
        typeof(GameObject)
    ])]
    private static void SetTokenValue_Postfix(
        Token tok)
    {
        if (tok.type != Token.TokenType.HardCoded) return;
        if (!IsValid((Token.HardCodedTokens)tok.id))
            return;
        Plugin.Logger.LogInfo("Token type: " + (Token.HardCodedTokens)tok.id);
        LogStackTrace();
    }

    private static bool IsValid(Token.HardCodedTokens token)
    {
        return token is Token.HardCodedTokens.ForcePsychogunOn or Token.HardCodedTokens.ForcePsychogunOff
            or Token.HardCodedTokens.ForcePsychogunOffInstant or Token.HardCodedTokens.ForcePsychogunOnInstant;
    }

    [HarmonyPostfix]
    [HarmonyPatch([
        typeof(Token.HardCodedTokens),
        typeof(int),
        typeof(Token.ValueOperator),
        typeof(bool),
        typeof(GameObject)
    ])]
    private static void SetTokenValue_Postfix(
        Token.HardCodedTokens hardToken)
    {
        if (!IsValid(hardToken))
            return;
        Plugin.Logger.LogInfo("Token type: " + hardToken);
        LogStackTrace();
    }

    private static void LogStackTrace()
    {
        Plugin.Logger.LogWarning(Environment.StackTrace);
    }
}
*/