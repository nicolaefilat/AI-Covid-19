﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Covid19.AIBehaviour.Behaviour.Configuration;
using Covid19.AIBehaviour.Behaviour.States;
using UnityEngine;
using UnityEngine.AI;

namespace Covid19.AIBehaviour.Behaviour
{
    public class AgentNPC : MonoBehaviour
    {
        public enum GroupType
        {
            Cook,
            Business,
        }

        public GroupType type;
        public AgentConfiguration agentConfiguration;

        [HideInInspector] public GameObject[] patrolPositions; // array holding patrol positions

        public GameObject
            posHolder; // This is used for an easier way to set patrol points, should be added to a SO in the feature

        public NavMeshAgent Agent { get; private set; }
        public Animator Animator { get; private set; }
        public MeetSystem MeetSystem { get; private set; }

        public GameObject rightHand;
        private readonly Stack<IBehaviour> _behaviours = new Stack<IBehaviour>();
        private readonly Dictionary<IBehaviour, Coroutine> _dictionary = new Dictionary<IBehaviour, Coroutine>();
        private AgentUI _agentUI;
        private IBehaviour _currentBehaviour;
        private InfectionSystem _infectionSystem;
        private Dictionary<GroupType, List<IBehaviour>> _altceva = new Dictionary<GroupType, List<IBehaviour>>();
        private List<System.Type> _basicActions = new List<System.Type>
        {
            typeof(MeetBehaviour),
            typeof(EatBeahaviour),
        };

        private void Start()
        {
            System.Type type = typeof(MeetBehaviour);
            // altceva.Add(GroupType.Cook, new List<IBehaviour>(TypingBehaviour), typeof(MeetBehaviour)));
            // altceva.Add(GroupType.Business, new List<IBehaviour>(TypingBehaviour, MeetBehaviour,));
            AgentManager.Instance.AddAgent(this);

            _agentUI = GetComponentInChildren<AgentUI>();

            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
            GetComponent<Rigidbody>().useGravity = false;

            SetBehaviour(GetComponent<IBehaviour>());
            MeetSystem = new MeetSystem(this);
            _infectionSystem = new InfectionSystem(this);
        }

        public bool IsCurrentBehaviour(IBehaviour behaviour)
        {
            if (_behaviours.Count == 0)
                return false;
            return _behaviours.Peek() == behaviour;
        }
        
        public void SetBehaviour(IBehaviour behaviour)
        {
            if (_currentBehaviour != null)
            {
                //Debug.Log($"Ending coroutine {_currentBehaviour} because we are setting up {behaviour} now",this);
                StopCoroutine(_dictionary[_currentBehaviour]);
                _currentBehaviour.Disable();
            }

            _currentBehaviour = behaviour; // set the current behaviour
            _behaviours.Push(_currentBehaviour); // push this behaviour to the stack
            _currentBehaviour.Enable(); // run initialization logic ( get the dependencies)

            Coroutine coroutine = StartCoroutine(_currentBehaviour.OnUpdate()); // cache the corutine to stop later
            //Debug.Log($"Putting the coroutine {_currentBehaviour} into the dictionary",this);
            _dictionary[_currentBehaviour] = coroutine;
        }

        public void RemoveBehaviour(IBehaviour behaviour)
        {
            StopCoroutine(_dictionary[behaviour]);
            behaviour.Disable();
            Destroy(behaviour as Object);
            _behaviours.Pop();
            if (_behaviours.Count > 0)
            {
                _currentBehaviour = _behaviours.Peek();
                _currentBehaviour.Enable();
                Coroutine coroutine = StartCoroutine(_currentBehaviour.OnUpdate());
                _dictionary[_currentBehaviour] = coroutine;
            }
        }

        public void StartInfection()
        {
            _infectionSystem.StartInfection();
        }

        private void OnDrawGizmos()
        {
            if (_behaviours.Count > 0 && _agentUI) _agentUI.gizmoActionNameHolder.name = _behaviours.Peek().ToString();
        }
    }
}