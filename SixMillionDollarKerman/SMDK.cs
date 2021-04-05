using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SixMillionDollarKerman
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SMDK : PartModule
    {
        // KSPEvent to toggle Million Dollar mode

        [KSPEvent(active = true, externalToEVAOnly = true, guiActive = true, guiActiveEditor = false, 
            isPersistent = false, guiName = "Activate Six Million Dollar Mode")]
        public void SixMillionDollars()
        {
            if (!isRich)
            {
                Events["SixMillionDollars"].guiName = "Active Standard Mode";
                isRich = true;
                SetMode();
            }
            else
            {
                Events["SixMillionDollars"].guiName = "Activate Six Million Dollar Mode";
                isRich = false;
                SetMode();
            }

        }

        [KSPField(isPersistant = true)]
        public bool isRich = false;

        public bool canActivate = false;
        public bool isJumping = false;
        public Dictionary<String, float> poorStore = new Dictionary<string, float>();
        public Dictionary<String, float> richStore = new Dictionary<string, float>();
        public float runVal;
        public float walkVal;
        public float jumpVal;
        public float clamberReachVal;
        public float ladderClimbVal;
        public float swimVal;
        public float stumbleT;
        public double recoverVal;
        public double alt;
        public Rigidbody rb;
        public KeyBinding sb;
        public KeyBinding fb;
        public KeyBinding bb;
        public KeyBinding lb;
        public KeyBinding riB;
        

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                sb = GameSettings.EVA_Jump;
                fb = GameSettings.EVA_forward;
                bb = GameSettings.EVA_back;
                lb = GameSettings.EVA_left;
                riB = GameSettings.EVA_right;
                runVal = 2.2f;
                walkVal = 0.8f;                     
                jumpVal = 0.3f;
                clamberReachVal = 0.9f;
                ladderClimbVal = 0.6f;
                swimVal = 0.8f;
                stumbleT = 3.5f;
                recoverVal = 3.0;
                alt = 0;

                poorStore.Add("runVal", runVal);                        // keep stock values safe for when million dollar mode off
                poorStore.Add("walkVal", walkVal);
                poorStore.Add("jumpVal", jumpVal);
                poorStore.Add("clamberReachVal", clamberReachVal);
                poorStore.Add("ladderClimbVal", ladderClimbVal);
                poorStore.Add("swimVal", swimVal);
                poorStore.Add("stumbleT", stumbleT);

                richStore.Add("runVal", 20.0f);                         // keep million dollar values for easy reference
                richStore.Add("walkVal", 5.0f);
                richStore.Add("jumpVal", 1.25f);
                richStore.Add("clamberReachVal", 5.0f);
                richStore.Add("ladderClimbVal", 5.0f);
                richStore.Add("swimVal", 5.0f);
                richStore.Add("stumbleT", 15f);
            }       
        }

        public void Update()
        {
            
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                return;
            }
            
            if (isRich)
            {
                
                if (!fb.GetKey(false) && !bb.GetKey(false) && !lb.GetKey(false) && !riB.GetKey(false))    // no direction key being held
                {
                    if (sb.GetKeyDown(false))                                                     // tap space
                    {
                        isJumping = true;
                        FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().maxJumpForce = 5.0f;       //dynamically set jump power
                    }

                }
                else
                {
                    isJumping = false;
                    FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().maxJumpForce = poorStore["jumpVal"];   //set back
                } 
            }
        }
           


        public void FixedUpdate()
        {
            if (isJumping && isRich)
            {
                Vessel ves = FlightGlobals.ActiveVessel;                    
                alt = ves.heightFromTerrain;                                
                rb = ves.GetComponent<Rigidbody>();                         
                double vSpeed = ves.verticalSpeed;                          
                
                float antiGravEffect = (float.Parse(vSpeed.ToString()) * -1f) / float.Parse(alt.ToString());  

                if (vSpeed < 0)                                         
                {
                    rb.AddRelativeForce(0f, antiGravEffect, 0f);        
                    rb.angularVelocity = Vector3.zero;                  
                }
                
            }

        }


        public void SetMode()
        {
            if (isRich && FlightGlobals.ActiveVessel.isEVA)          // set million dollar
            {
                KerbalEVA kE = FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>();
                kE.runSpeed = richStore["runVal"];
                kE.walkSpeed = richStore["walkVal"];
                kE.clamberReach = richStore["clamberReachVal"];
                kE.ladderClimbSpeed = richStore["ladderClimbVal"];
                kE.swimSpeed = richStore["swimVal"];
                kE.stumbleThreshold = richStore["stumbleT"];
                kE.recoverTime = 0.0001;
                kE.DebugFSMState = true;
                kE.splatEnabled = false;
            }

            else if (!isRich && FlightGlobals.ActiveVessel.isEVA)
            {
                KerbalEVA kE = FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>();        // set default
                kE.runSpeed = poorStore["runVal"];
                kE.walkSpeed = poorStore["walkVal"];
                kE.maxJumpForce = poorStore["jumpVal"];
                kE.clamberReach = poorStore["clamberReachVal"];
                kE.ladderClimbSpeed = poorStore["ladderClimbVal"];
                kE.swimSpeed = poorStore["swimVal"];
                kE.stumbleThreshold = poorStore["stumbleT"];
                kE.recoverTime = 3.0;
                kE.splatEnabled = true;
            }
        }
    }

   
}
