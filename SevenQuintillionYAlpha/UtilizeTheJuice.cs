using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SevenQuintillionYAlpha
{
    class UtilizeTheJuice
    {
        private static UtilizeTheJuice singletonInstance;
        public static UtilizeTheJuice Instance
        {
            get
            {
                singletonInstance = singletonInstance ?? new UtilizeTheJuice();
                return singletonInstance;
            }
        }

        public void GetLocalGuy() => eLocalPlayer = UnityEngine.Object.FindObjectOfType<EntityPlayerLocal>();

        public void GetNonLocalGuys() => eNonLocalPlayers = UnityEngine.Object.FindObjectsOfType<EntityPlayer>();

        public string[] GetNonLocalGuysNames() => eNonLocalPlayers.Select(eP => eP.EntityName).ToArray();

        public void UpdateTheJuicer()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
                JuicyMenu.Instance.ToggleMenuVisibilty();

            if (eLocalPlayer == null)
                return;

            if (eTargetPlayer == null)
                eTargetPlayer = eLocalPlayer;

            if (Input.GetKeyDown(KeyCode.F5) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyItem])
                GiveTheJuicer();

            if (JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyKills])
                GiveKills();

            if (JuicyMenu.Instance.bAimer && Input.GetKey(KeyCode.LeftAlt))
                JustSkillForsenCD();

            if (Input.GetKeyDown(KeyCode.F6) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyEXP])
                GrantEXP();

            if (Input.GetKeyDown(KeyCode.F6) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplySP])
                GrantSP();

            if (JuicyMenu.Instance.bApply[(int)ApplyElements.Apply0D])
                Grant0D();

            if (JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyGetCoordinates])
                GetPosition();

            if (Input.GetKeyDown(KeyCode.F7) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyTeleport])
                GotoSeekedPosition();

            if (Input.GetKeyDown(KeyCode.F8) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyDeNerf])
            {
                DeNerf();
                StringBuilder stringBuilder = new StringBuilder();
                foreach(string ItemN in ItemClass.ItemNames)
                {
                    stringBuilder.Append(ItemN + "\n");
                }
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "juice.txt"), stringBuilder.ToString());
                stringBuilder.Clear();
            }

            if(Input.GetKeyDown(KeyCode.End) || JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyUnload])
            {
                JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyUnload] = false;
                Juicer.ExtractTheJuicer();
            }

            if(JuicyMenu.Instance.bApply[(int)ApplyElements.ApplySwitch])
            {
                if (JuicyMenu.Instance.bLocalOnly)
                {
                    GetLocalGuy();
                    eTargetPlayer = eLocalPlayer;
                }
                else
                {
                    GetNonLocalGuys();
                    eTargetPlayer = eNonLocalPlayers[JuicyMenu.Instance.iSelected];
                }

                JuicyMenu.Instance.bApply[(int)ApplyElements.ApplySwitch] = false;
            }
        }

        private void Grant0D()
        {
            eLocalPlayer.Died = 0;
            JuicyMenu.Instance.bApply[(int)ApplyElements.Apply0D] = false;
        }

        private void GiveTheJuicer()
        {
            if (int.TryParse(JuicyMenu.Instance.szAmmount, out int iAmmount))
            {
                ItemValue vSeeked = ItemClass.GetItem(JuicyMenu.Instance.szIName);
                ItemStack sSeeked = new ItemStack(vSeeked, iAmmount);
                eTargetPlayer.inventory.AddItem(sSeeked);
            }
            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyItem] = false;
        }

        private void GiveKills()
        {
            if (int.TryParse(JuicyMenu.Instance.szAddKills, out int iKills))
                eTargetPlayer.KilledZombies += iKills;

            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyKills] = false;
        }

        private Vector3 GetRotation(EntityAlive eEnemy, float flMaxTol)
        {
            Vector3 eEnemyHead = eEnemy.getHeadPosition(), LocalHead = eLocalPlayer.getHeadPosition();
            float num = eEnemyHead.x - LocalHead.x;
            float num2 = eEnemyHead.z - LocalHead.z;
            float num3 = LocalHead.y - eEnemyHead.y + JuicyMenu.Instance.flAimOffset;
            float num4 = Mathf.Sqrt(num * num + num2 * num2);
            float intendedRotation = -(Mathf.Atan2(num2, num) * 180.0f / 3.141592653589793f) + 90f;
            float intendedRotation2 = -(Mathf.Atan2(num3, num4) * 180.0f / 3.141592653589793f);
            float rotX = EntityAlive.UpdateRotation(eLocalPlayer.rotation.x, intendedRotation2, flMaxTol);
            float rotY = EntityAlive.UpdateRotation(eLocalPlayer.rotation.y, intendedRotation, flMaxTol);
            return new Vector3(rotX, rotY, 0f);
        }

        private bool IsAnEnemy(EntityAlive eEntity)
        {
            return ((eEntity is EntityEnemy) || (eEntity is EntityPlayer && JuicyMenu.Instance.bAimPlayers));
        }

        private void FindNewTarget(float flMaxTol)
        {
            float flBestADistance = flMaxTol * 2;
            bool bEnemyFound = false;
            Vector3? vecBestRot = null;
            EntityAlive eBestTarget = null;

            eLocalPlayer.world.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(eLocalPlayer.position, Vector3.one * eLocalPlayer.GetSeeDistance()), liEnemiesAround);
            for (int i = 0; i < liEnemiesAround.Count; i++)
            {
                EntityAlive eEnemy = liEnemiesAround[i] as EntityAlive;

                if (!IsAnEnemy(eEnemy) && !(eEnemy is EntityAnimal))
                    continue;

                if (eEnemy is EntityAnimal && !(eEnemy is EntityEnemyAnimal) && bEnemyFound)
                    continue;

                if (eEnemy.IsDead())
                    continue;

                Vector3 vecRot = GetRotation(eEnemy, flMaxTol);
                float flADistance = Mathf.Sqrt(Mathf.Pow(vecRot.x - eLocalPlayer.rotation.x, 2f) + Mathf.Pow(vecRot.y - eLocalPlayer.rotation.y, 2f));

                if (flADistance > flMaxTol)
                    continue;

                if (flADistance < flBestADistance || !bEnemyFound)
                {
                    flBestADistance = flADistance;
                    vecBestRot = vecRot;
                    eBestTarget = eEnemy;
                }

                if (IsAnEnemy(eEnemy)) bEnemyFound = true;
            }

            if (vecBestRot != null)
                eLocalPlayer.SetRotation(vecBestRot.Value);

            eCurrentTarget = eBestTarget;

            liEnemiesAround.Clear();
        }

        private void JustSkillForsenCD()
        {
            float flMaxTol = eLocalPlayer.GetMaxViewAngle() * 0.25f;
            
            if (eCurrentTarget == null
                || eCurrentTarget.IsDead()
                || eLocalPlayer.GetDistance(eCurrentTarget) > 1.73205080757f * eLocalPlayer.GetSeeDistance())
            {
                FindNewTarget(flMaxTol);
                return;
            }

            Vector3 vecRot = GetRotation(eCurrentTarget, flMaxTol);
            float flADistance = Mathf.Sqrt(Mathf.Pow(vecRot.x - eLocalPlayer.rotation.x, 2f) + Mathf.Pow(vecRot.y - eLocalPlayer.rotation.y, 2f));

            if (flADistance > 2f)
            {
                FindNewTarget(flMaxTol);
                return;
            }

            eLocalPlayer.SetRotation(vecRot);
        }

        private void GrantEXP()
        {
            if(int.TryParse(JuicyMenu.Instance.szDreamEXP, out int iDEXP))
                eTargetPlayer.Progression.AddLevelExp(iDEXP);

            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyEXP] = false;
        }

        private void GrantSP()
        {
            if (int.TryParse(JuicyMenu.Instance.szDreamSP, out int iSP))
                eTargetPlayer.Progression.SkillPoints = iSP;

            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplySP] = false;
        }
        private void GetPosition()
        {
            JuicyMenu.Instance.szTargetCoordinates[0] = eTargetPlayer.position.x.ToString();
   
            JuicyMenu.Instance.szTargetCoordinates[1] = eTargetPlayer.position.y.ToString();
   
            JuicyMenu.Instance.szTargetCoordinates[2] = eTargetPlayer.position.z.ToString();
   
            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyGetCoordinates] = false;
        }

        private void GotoSeekedPosition()
        {
            if (float.TryParse(JuicyMenu.Instance.szTargetCoordinates[0], out float x)
                && float.TryParse(JuicyMenu.Instance.szTargetCoordinates[1], out float y)
                && float.TryParse(JuicyMenu.Instance.szTargetCoordinates[2], out float z))
            {
                Vector3 vecTargetPosition = new Vector3(x, y, z);
                if (eTargetPlayer.AttachedToEntity != null)
                    eTargetPlayer.AttachedToEntity.SetPosition(vecTargetPosition, true);
                else
                    eTargetPlayer.SetPosition(vecTargetPosition, true);

                eTargetPlayer.Respawn(RespawnType.Teleport);

                if (y >= 0f)
                    ThreadManager.StartCoroutine(SetVerticalPosition(vecTargetPosition, null));
            }
            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyTeleport] = false;
        }

         private IEnumerator SetVerticalPosition(Vector3 _pos, Vector3? _viewDirection)
         {
            while (!eTargetPlayer.Spawned)
            {
                yield return null;
            }
            if (eTargetPlayer.AttachedToEntity != null)
            {
                eTargetPlayer.AttachedToEntity.SetPosition(_pos, true);
            }
            else
            {
                eTargetPlayer.SetPosition(_pos, true);
            }
            if (_viewDirection != null)
            {
                eTargetPlayer.SetRotation(_viewDirection.Value);
            }
            yield break;
         }

        private void DeNerf()
        {
            string[] szTargetNerfs =
            {
                "buffdysenterycatchfood",
                "buffdysenterycatchdrink",
                "buffdysenterymain",
                "buffdysenteryaddcure",
                "buffdysenterycuredisplay",
                "buffdysentery01untreated",
                "buffdysentery01getbetter",
                "buffdysentery01untreateddiarrhea",
                "buffdysentery01getbetterdiarrhea",

                "buffinfectioncatch",
                "buffinfectionmain",
                "buffinfectiongetsworse",
                "buffinfectionaddcure",
                "buffinfectioncuredisplay",
                "buffinfection01untreated",
                "buffinfection01getbetter",
                "buffinfection02untreated",
                "buffinfection02getbetter",
                "buffinfection03untreated",
                "buffinfection03getbetter",
                "buffinfection04",

                "buffinjurybleedingone",
                "buffinjurybleedingtwo",
                "buffinjurybleedingbarbedwire",
                "buffinjurybleeding",
                "buffinjurybleedingparticle",
                "buffabrasioncatch",
                "buffinjuryabrasion",
                "buffinjuryabrasiontreated",
                "buffplayerfallingdamage",
                "bufflegsprainedchtrigger",
                "bufflegsprainedfalltrigger",
                "bufflegsprained",
                "bufflegbroken",
                "bufflegsplinted",
                "bufflegcast",
                "buffarmsprainedchtrigger",
                "buffarmsprained",
                "buffarmbroken",
                "buffarmsplinted",
                "buffarmcast",
                "buffbrokenlimbstatus",
                "bufflacerationbleedingstatus",
                "bufffatiguedtrigger",
                "bufffatigued",
                "bufffatiguegetsworse",
                "bufflaceration",
                "buffinjurystunned01shotgun",
                "buffinjurystunned02shotgun",
                "buffinjurystunned03shotgun",
                "buffinjurystunned00",
                "buffinjurystunned01chtrigger",
                "buffinjurystunned01",
                "buffinjurystunned02",
                "buffinjurystunned03",
                "buffinjurystunned01cooldown",
                "buffinjuryconcussion",
                "buffinjuryconcussionblur",
                "buffinjuryconcussiongetsworse"
            };

            foreach (var Nerf in szTargetNerfs)
            {
                if (eLocalPlayer.Buffs.HasBuff(Nerf))
                    eLocalPlayer.Buffs.RemoveBuff(Nerf);
            }
            /*
            StringBuilder stringBuilder = new StringBuilder();
            foreach(string szBuffName in BuffManager.Buffs.Keys)
            {
                stringBuilder.Append(szBuffName + "\n");
            }
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "elbuffets.txt"), stringBuilder.ToString());
            stringBuilder.Clear();
            */
            JuicyMenu.Instance.bApply[(int)ApplyElements.ApplyDeNerf] = false;
        }

        private EntityPlayerLocal eLocalPlayer = null;
        private EntityPlayer eTargetPlayer = null; 
        private EntityPlayer[] eNonLocalPlayers = null;
        private List<Entity> liEnemiesAround = new List<Entity>();
        private EntityAlive eCurrentTarget = null;
    }
}
