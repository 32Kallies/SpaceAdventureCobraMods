using System;
using TrainerMod.Enums;
using UnityEngine;

namespace TrainerMod;

public class TrainerStatManager : MonoBehaviour
{
    public CobraCharacter cobra;

    private bool _crystalBoyInstanceExists;

    private static float? _defaultRevolverDamage;
    private static float? _defaultCigarDamage;

    private void Start()
    {
        Plugin.PsychogunDamage.SettingChanged += OnPsychogunDamageChanged;
        Plugin.PsychogunPenetratingShotDamage.SettingChanged += OnPsychogunPenetratingShotDamageChanged;
        Plugin.CurvedShotMode.SettingChanged += OnCurvedShotModeChanged;
        Plugin.UnlimitedSuperShots.SettingChanged += OnUnlimitedSuperShotsChanged;
        Plugin.RevolverDamage.SettingChanged += OnRevolverDamageChanged;
        Plugin.CigarDamage.SettingChanged += OnCigarDamageChanged;
        Plugin.UnlimitedCigars.SettingChanged += OnUnlimitedCigarsChanged;
        Plugin.CobraHealth.SettingChanged += OnCobraHealthChanged;

        OnPsychogunDamageChanged(null, null);
        OnPsychogunPenetratingShotDamageChanged(null, null);
        OnCurvedShotModeChanged(null, null);
        OnUnlimitedSuperShotsChanged(null, null);
        OnRevolverDamageChanged(null, null);
        OnCigarDamageChanged(null, null);
        OnUnlimitedCigarsChanged(null, null);
        OnCobraHealthChanged(null, null);
    }

    private void Update()
    {
        if (Plugin.UnlimitedSuperShots.Value)
        {
            cobra.ultraEnergy = 2;
        }

        if (Plugin.UnlimitedCigars.Value)
        {
            if (cobra.explosiveCigarParams.remainingCigar < cobra.explosiveCigarParams.maxCigar)
            {
                cobra.explosiveCigarParams.remainingCigar = cobra.explosiveCigarParams.maxCigar;
            }
        }

        if (Plugin.CurvedShotMode.Value == GuidedShotMode.Unlimited)
        {
            cobra.psychoEnergy = cobra.energy.psychoEnergyMax;
        }

        bool crystalBoyExists = NmiCrystalBowie.Instance != null;
        if (crystalBoyExists != _crystalBoyInstanceExists)
        {
            _crystalBoyInstanceExists = crystalBoyExists;
            OnRevolverDamageChanged(null, null);
        }
    }

    private void OnPsychogunDamageChanged(object sender, EventArgs e)
    {
    }

    private void OnPsychogunPenetratingShotDamageChanged(object sender, EventArgs e)
    {
    }

    private void OnCurvedShotModeChanged(object sender, EventArgs e)
    {
        switch (Plugin.CurvedShotMode.Value)
        {
            case GuidedShotMode.Normal:
                cobra.energy.isLimitedGuidedShot = true;
                break;
            case GuidedShotMode.Unlimited:
                cobra.energy.isLimitedGuidedShot = false;
                break;
            case GuidedShotMode.SingleUse:
                cobra.energy.isLimitedGuidedShot = true;
                break;
        }
    }
    
    private void OnUnlimitedSuperShotsChanged(object sender, EventArgs e)
    {
    }

    private void OnRevolverDamageChanged(object sender, EventArgs e)
    {
        var projectile = cobra.dependencies.revolverShot.GetComponent<Projectile>();
        if (!_defaultRevolverDamage.HasValue)
        {
            _defaultRevolverDamage = projectile.damage;
        }

        float newRevolverDamage;
        if (Plugin.RevolverDamage.Value < 0)
        {
            newRevolverDamage = _defaultRevolverDamage.Value;
        }
        else if (_crystalBoyInstanceExists && Plugin.LimitRevolverDamageAgainstCrystalBoy.Value)
        {
            newRevolverDamage = Mathf.Min(_defaultRevolverDamage.Value, Plugin.RevolverDamage.Value);
        }
        else
        {
            newRevolverDamage = Plugin.RevolverDamage.Value;
        }

        projectile.damage = newRevolverDamage;
    }

    private void OnCigarDamageChanged(object sender, EventArgs e)
    {
        var explosion = cobra.explosiveCigarParams.prefab.GetComponent<ExplosiveCigar>().explosion.transform
            .Find("OneShotExplosion")
            .GetComponent<OneShotExplosion>();

        if (!_defaultCigarDamage.HasValue)
        {
            _defaultCigarDamage = explosion.dmg;
        }
        
        explosion.dmg = Plugin.CigarDamage.Value < 0f ? _defaultCigarDamage.Value : Plugin.CigarDamage.Value;
    }

    private void OnUnlimitedCigarsChanged(object sender, EventArgs e)
    {
        
    }

    private void OnCobraHealthChanged(object sender, EventArgs e)
    {
        cobra.UpdateLifeMax(true);
    }
}