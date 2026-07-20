public class WeaponRules
{
    //ALPHABETICAL LIST OF WEAPON RULES!

    //Accurate x: Retain x attack dice as normal successes without rolling (Pre-roll - Retain logic)
    //Balanced: Reroll one attack dice (After roll - allows reroll)
    //Blast x: Attack another visible unit with another roll within x inches of initial target (After attack evaluation - allows extra targets)
    //Brutal: Enemy unit can only block with critical successes (On attack evaluation - affects enemy unit)
    //Ceaseless: Reroll duplicate values (After roll - allows reroll)
    //Devastating x: Retained critical hits deal x damage to units immediately, they are not discarded (After roll - when retained - affects enemy unit)
    //Heavy (Condition): Cannot use attack if the unit has moved at all, other than condition movement such as dash (Pre-roll - Condition to allow attack)
    //Hot: Roll D6 after attack. Result < weapon hit stat, then unit takes result*2 damage (After attack evaluation - rolls)
    //Lethal x: Successful dice rolls > x are crits (After roll - when retained)
    //Limited x: Weapon can only be used x times (After attack evaluation - logic)
    //Piercing x: Defender collects x less defense dice (Pre-roll - affects enemy unit)
    //Piercing crits x: Same as piercing but only on retained crits (After roll - when retained - affects enemy unit)
    //Punishing: If you retain a crit success, one fail can be retained as a normal success (After roll - when retained - allows additional retention)
    //Range x: Only enemy units within x range are valid targets (Pre-roll - condition to allow attack)
    //Relentless: Reroll any of your attack dice (After roll - allows reroll)
    //Rending: If you retain a critical success, you can retain a normal success as a crit (After roll - when retained - allows additional retention)
    //Saturate: Defender cannot retain cover saves (Pre-roll - affects enemy unit)
    //Seek (Cover type): When selecting valid targets, enemies cannot use (cover type) for cover (Pre-roll - affects valid targeting - affects enemy unit)
    //Severe: If you don't retain any critical successes, change a normal success to a crit (After roll - when retaining - additional retention)
    //Shock: First time you strike with a crit in each sequence discard an opponent's unresolved normal success or crits if none (On attack evaluation - affects enemy unit)
    //Silent: Unit can perform shoot while it is concealed (Pre-roll - condition on unit state)
    //Stun: If you retain any critical successes, subtract 1APL from enemy unit until its next activation (After roll - when retaining - affects enemy unit)
    //Torrent x: Attack multiple targets within x of initial if they are valid targets (After attack evaluation - allows extra attack)


    //WEAPON RULES SORTED INTO TIMING/CONDITION CATEGORIES

    //Pre-roll rules
    //Accurate x: (Retain logic)
    //Heavy (Condition): (Condition to allow attack)
    //Range x: (Condition to allow attack)
    //Piercing x: (Affects enemy unit)
    //Saturate: (Affects enemy unit)
    //Seek (Cover type): (Affects valid targeting - Affects enemy unit)
    //Silent: (Condition on unit state)

    //After roll rules
    //Balanced: (Allows rerolls)
    //Ceaseless: (Allows rerolls)
    //Relentless: (Allows rerolls)
    //Lethal x: (When retained - crit modification)
    //Devastating x: (When retained - affects enemy unit)
    //Piercing crits x: (When retained - affects enemy unit)
    //Stun: (When retained - affects enemy unit)
    //Punishing: (When retained - allows additional retention)
    //Rending: (When retained - allows additional retention)
    //Severe: (When retained - allows additional retention)

    //On attack evaluation
    //Brutal: (Affects enemy unit)
    //Shock: (Affects enemy unit)

    //After attack evaluation
    //Blast x: (Allows extra targets)
    //Torrent x: (Allows extra targets)
    //Hot: (A roll effect on unit)
    //Limited x: (Logic on weapon capability)

    //Rule logics
    
    [System.Serializable]
    public class Balanced : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterRoll;

        public void Execute(InformationPackage context)
        {
            //Reroll one attack dice
        }
        
        public override string ToString() => "Balanced";
    }

    [System.Serializable]
    public class Range : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public int range;

        public void Execute(InformationPackage context)
        {
            //Only enemy units within x range are valid targets
        }
        
        public override string ToString() => $"Range: {range}";
    }

    [System.Serializable]
    public class Piercing : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public int piercingX;

        public void Execute(InformationPackage context)
        {
            //Defender collects x less defense dice
        }
        
        public override string ToString() => $"Piercing: {piercingX}";
    }

    [System.Serializable]
    public class Hot : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterAttackEvaluation;

        public void Execute(InformationPackage context)
        {
            //Roll D6 after attack
        }
        
        public override string ToString() => "Hot";
    }

    [System.Serializable]
    public class Lethal : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterRoll;

        public int lethalX;

        public void Execute(InformationPackage context)
        {
            //Successful dice rolls > x are crits
        }
        
        public override string ToString() => $"Lethal: {lethalX}";
    }

    [System.Serializable]
    public class Brutal : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AttackEvaluation;

        public void Execute(InformationPackage context)
        {
            //Enemy unit can only block with critical successes
        }
        
        public override string ToString() => "Brutal";
    }

    [System.Serializable]
    public class Saturate : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public void Execute(InformationPackage context)
        {
            //Defender cannot retain cover saves
        }
        
        public override string ToString() => "Saturate";
    }

    [System.Serializable]
    public class Torrent : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterAttackEvaluation;

        public int torrentX;

        public void Execute(InformationPackage context)
        {
            //Attack multiple targets within x of initial if they are valid targets
        }
        
        public override string ToString() => $"Torrent: {torrentX}";
    }

    [System.Serializable]
    public class PiercingCrit : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterRoll;

        public int pCritX;

        public void Execute(InformationPackage context)
        {
            //Same as piercing but only on retained crits
        }
        
        public override string ToString() => $"PiercingCrit {pCritX}";
    }
    
    //Custom one to know if this is a melee weapon
    [System.Serializable]
    public class Melee : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public void Execute(InformationPackage context)
        {
            //Check that fight conditions are met to use this weapon
        }
        
        public override string ToString() => "Melee";
    }
    
    [System.Serializable]
    public class Shock : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AttackEvaluation;

        public void Execute(InformationPackage context)
        {
            //First time you strike with a crit in each sequence discard an opponent's unresolved normal success or crits if none
        }
        
        public override string ToString() => "Shock";
    }
    
    [System.Serializable]
    public class Stun : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterRoll;

        public void Execute(InformationPackage context)
        {
            //If you retain any critical successes, subtract 1APL from enemy unit until its next activation
        }
        
        public override string ToString() => "Stun";
    }
    
    [System.Serializable]
    public class Heavy : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public string condition;
        
        public void Execute(InformationPackage context)
        {
            //Cannot use attack if the unit has moved at all, other than condition movement such as dash
        }
        
        public override string ToString() => $"Heavy: {condition}";
    }
    
    [System.Serializable]
    public class Silent : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;
        
        public void Execute(InformationPackage context)
        {
            //Unit can perform shoot while it is concealed
        }
        
        public override string ToString() => "Silent";
    }
    
    [System.Serializable]
    public class Seek : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.PreRoll;

        public string coverType;
        
        public void Execute(InformationPackage context)
        {
            //When selecting valid targets, enemies cannot use (cover type) for cover
        }
        
        public override string ToString() => $"Seek: {coverType}";
    }
    
    [System.Serializable]
    public class Blast : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterAttackEvaluation;

        public int blastX;
        
        public void Execute(InformationPackage context)
        {
            //Attack another visible unit with another roll within x inches of initial target
        }
        
        public override string ToString() => $"Blast: {blastX}";
    }
    
    [System.Serializable]
    public class Devastating : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterAttackEvaluation;

        public int devastatingX;
        
        public void Execute(InformationPackage context)
        {
            //Retained critical hits deal x damage to units immediately, they are not discarded
        }
        
        public override string ToString() => $"Devastating: {devastatingX}";
    }
    
    [System.Serializable]
    public class Ceaseless : IWeaponRule
    {
        public AttackTimings Step => AttackTimings.AfterRoll;
        
        public void Execute(InformationPackage context)
        {
            //Reroll duplicate values
        }
        
        public override string ToString() => "Ceaseless";
    }
    
}