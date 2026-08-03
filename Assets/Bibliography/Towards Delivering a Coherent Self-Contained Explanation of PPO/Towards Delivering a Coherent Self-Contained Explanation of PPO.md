| Towards     | Delivering  | a Coherent | Self-Contained      |     |
| ----------- | ----------- | ---------- | ------------------- | --- |
| Explanation | of Proximal |            | Policy Optimization |     |
|             | Master’s    | Research   | Project             |     |
Daniel Bick
daniel.bick@live.de
|     |     | August 15, | 2021 |     |
| --- | --- | ---------- | ---- | --- |
Supervisors: Prof. dr. H. Jaeger (Artificial Intelligence, University of Groningen),
dr. M.A. Wiering (Artificial Intelligence, University of Groningen)
Artificial Intelligence
|     |     | University | of Groningen, | The Netherlands |
| --- | --- | ---------- | ------------- | --------------- |

Abstract
Reinforcement Learning (RL), and these days particularly Deep Reinforcement Learning (DRL), is
concerned with the development, study, and application of algorithms that are designed to accomplish
some arbitrary task by learning a decision-making strategy that aims for maximizing a cumulative per-
formance measure. While this class of machine learning algorithms has become increasingly successful
onavarietyoftasksoverthelastyears,someofthealgorithmsdevelopedinthisfieldaresub-optimally
documented. One example of a DRL algorithm being sub-optimally documented is Proximal Policy
Optimization (PPO), which is a so-called model-free policy gradient method (PGM). Since PPO is a
state-of-the-art representative of the important class of PGMs, but can hardly be understood from only
consulting the paper having introduced it, this report aims for explaining PPO in detail. Thereby, the
report shines a light on many concepts generalizing to the wider field of PGMs. Also, a reference im-
plementation of PPO has been developed, which will shortly be introduced and evaluated. Lastly, this
report examines the limitations of PPO and quickly touches upon the topic of whether DRL might lead
to the emergence of General Artificial Intelligence in the future.
1

Contents
1 Introduction 3
2 Preliminaries 4
2.1 Reinforcement Learning . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 4
2.2 Policy Gradient Methods . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 7
3 Proximal Policy Optimization 9
3.1 Overview . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 10
3.2 Generation of Actions . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 13
3.2.1 Continuous Action Spaces . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 13
3.2.2 Discrete Action Spaces . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 14
3.3 Computation of Target Values. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 14
3.3.1 Target State Values . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 15
3.3.2 Advantage Estimates . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 15
3.4 Explanation of Policy Network’s Main Objective Function LCLIP . . . . . . . . . . . . . . . . 16
3.5 Exploration Strategies . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 19
3.5.1 Exploration in Continuous Action Spaces . . . . . . . . . . . . . . . . . . . . . . . . . 20
3.5.2 Exploration in Discrete Action Spaces . . . . . . . . . . . . . . . . . . . . . . . . . . . 20
3.6 Back-Propagation of Overall Objective Function . . . . . . . . . . . . . . . . . . . . . . . . . 21
3.6.1 Back-Propagation of LCLIP . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 22
3.6.2 Continuing Back-Propagation of LCLIP in Continuous Action Spaces. . . . . . . . . . 23
3.6.3 Continuing Back-Propagation of LCLIP in Discrete Spaces . . . . . . . . . . . . . . . 23
3.6.4 Back-Propagation of state-value Network’s Objective Function . . . . . . . . . . . . . 24
3.6.5 Back-Propagation Entropy Bonus in Discrete Action Spaces . . . . . . . . . . . . . . . 24
3.7 Pseudocode . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 25
4 Reference Implementation 25
4.1 Description of Reference Implementation . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 25
4.2 Evaluation of Reference Implementation . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 28
5 Considerations and Discussion of PPO 30
5.1 Critical Considerations concerning PPO . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 31
5.2 RL in the Context of Artificial Intelligence (AI) . . . . . . . . . . . . . . . . . . . . . . . . . . 32
6 Conclusion 33
2

1 Introduction
Reinforcement Learning (RL) refers to a class of machine learning algorithms which can be trained by re-
peatedly being told how good or bad their recent behavior in their environment has been. Those algorithms
can be trained without (or, depending on the point of view, minimal [1]) prior knowledge of the task they
mayhavetoaccomplishinthefuture[2]. AsometimesveryvaluableaspectisthatRLalgorithms,oragents,
may be trained without there being a clear definition of the task to be learned by an agent. The only
requirements for training an RL agent encompass an environment that the agent can observe and interact
with, as well as the numeric so-called reward signal telling the agent about the appropriateness of its recent
behavior [3]. A more formal definition of RL will be given in the succeeding section. For now it suffices to
say that the essence of training an RL agent is to learn some function, called policy, which tells the agent
whichactiontoperforminanygivenstatetheagentencountersinitsenvironment[3]. Thepolicyistrained
soastomaximizethesumofrewardsthattheagentexpectstoobtaininthefuturegivenitschoiceofaction
in every state the agent encounters [2, 3].
In the past, various kinds of functions have been used as policies. In some approaches, policies were
realized as look-up tables [3], while other approaches used linear functions combined with a set of hand-
crafted features that were used to encode information from raw state representations observed by an agent,
as reported in [2]. However, these approaches were susceptible to too few or poor hand-crafted feature rep-
resentations being provided [2].
In 2013, a team at DeepMind successfully demonstrated for the first time that it was possible to success-
fullytrainRLagents,whosepolicieswererealizedasartificialneuralnetworks(NNs)[2],therebymotivating
a lot of research into this direction. Since the NNs commonly being employed in this field are so-called
deep NN architectures, the resulting approach to RL was named Deep Reinforcement Learning (DRL). In
this paper, the two terms RL and DRL will commonly be used interchangeably, since DRL has become the
predominant approach to RL. Note that DRL architectures commonly do not rely on the use of pre-defined,
hand-craftedfeaturerepresentations,butcommonlylearnthemselvestoextractusefulfeaturesfromprovided
raw state representations throughout the course of training using stochastic gradient descent [2].
Since its introduction, a lot of research has been conducted in the field of DRL. Recurring themes in the
study of DRL are variations in how agents map from states to actions [4, 2] and whether they learn some
explicit representation of their environment [5] or not [6]. Other lines of research are concerned with the
question how to balance an agent’s curiosity for exploring new states and the agent’s tendency to exploit
the behavior that the agent has learned already over time [7, 2, 8].
Some of the major achievements in the field of DRL are as follows. In 2013, the authors of [2] demon-
strated for the first time that it was possible to train DRL agents on playing Atari games [9] without these
agents having any prior knowledge about the games they were trained on. Later, a follow-up publication
on this research was published in Nature [1]. These agents’ policy networks consumed sequences of raw
images representing a game’s screen over the last few time-steps and produced corresponding Q-values, i.e.
estimates of how good each available action would be in a given state. According to [1], the trained agents
achievedgameplayingperformancethatwascomparabletothatoftrainedhumanplayersacross49different
Atari games. Note that these agents used Convolutional Neural Networks [10] as their policy networks and
that they were trained end-to-end using stochastic gradient descent and learning principles from the field
of RL. Partly drawing upon DRL, in 2016 for the first time an agent excelled in the game of Go, beating
even human expert players [11]. Later, in 2017, this approach was improved to work even in the absence of
human knowledge during training [12] and, in 2018, generalized to also work for other games like chess and
shogi [13]. In 2019, a DRL agent was taught to master the game Dota 2 [14].
InspiteofallthegreatprogressmadeinthefieldofDRLoverthelastyears,someofthepaperspublished
in this field suffer from sub-optimally documented methods and/or algorithms they propose or utilize. In
order to understand the content of those publications, a substantial amount of prior knowledge from the
field of DRL is required.
One example of a paper proposing a sub-optimally documented DRL algorithm is the one proposing
the Proximal Policy Optimization (PPO) [6] algorithm. While the PPO algorithm itself is still, even years
3

after its invention, a state-of-the-art DRL algorithm [15], understanding it from the paper proposing it, [6],
requires extensive prior knowledge in the field of DRL.
PPOisaso-calledpolicygradientmethod(PGM)[6], whichmeansthattheagent’spolicymapsdirectly
from state representations to actions to be performed in an experienced state [4, 16]. PPO’s main demar-
cating feature is its objective function on which stochastic gradient ascent is performed in order to train
the policy network being either a NN or RNN [6]. The objective function is special in that it is designed
to allow for multiple epochs of weight updates on freshly sampled training data, which has commonly been
associated with training instablities in many other policy gradient methods [6]. Thereby, PPO allows for
training data being used more efficiently than in many other previous PGM methods [6].
In order to make the powerful PPO algorithm, as well as related methods, more accessible to a wider
audience, this report focuses on providing a comprehensible and self-contained explanation of the PPO al-
gorithm and its underlying methodology. When I set out to compile the contents for this paper, the task
appeared seemingly simple. While it was clear from the beginning that some background-research had to
be conducted to explain PPO in sufficiently more detail than in the original paper, it was not expected how
difficult compiling the following contents would eventually turn out to be. While the original paper on PPO
largelyfocusesonsomeparticularitiesofPPOandhowPPOisdifferentfromsomerelatedDRLalgorithms,
the aforementioned paper seemingly assumes a reader’s complete knowledge about the general working of
policy gradient methods (PGMs), thus not making any serious effort in explaining the fundamental proce-
dure upon which PPO rests. Consequently, a lot of my effort had to be spent on understanding the field
of PGMs in the first place, before being able to distil a clear picture of how PPO is different from other
vanillaPGMs. Inthisway,largepartsabouttheworkingofPPObecameapparent. Lastbutnotleast,some
final uncertainties concerning the working of PPO had to be ruled out by consulting a provided reference
implementation of PPO1 offered by OpenAI.
Inthefollowing, Section2formallyintroducesReinforcementLearning, aswellasPolicyGradientMeth-
ods, and establishes the basic notation used throughout the rest of this report. In Section 3, the PPO
algorithm will be presented in minute detail. A custom reference implementation will be introduced in
Section 4, followed by Section 5 discussing the algorithm, its shortcomings, potential improvements, and
the question whether the methods presented in this paper might lead to the emergence of General Atificial
Intelligence at some point in the future. This paper concludes with Section 6.
2 Preliminaries
This section will introduce some of the preliminary knowledge needed for understanding the working of the
PPO algorithm. First, Reinforcement Learning (RL) and Deep RL (DRL) will be introduced in Section 2.1.
Afterwards, policy gradient methods (PGMs) will be introduced in Section 2.2.
2.1 Reinforcement Learning
In Reinforcement Learning (RL), an algorithm, or agent, learns from interactions with its environment how
to behave in the given environment in order to maximize some cumulative reward metric [3]. An agent’s
decision-making strategy, which is also called policy and from which the agent’s behavior directly follows,
defines the way how the agent maps perceived environmental states to actions to be performed in these per-
ceivedstates[3]. Anenvironmentalstate,orsimplystate,referstoarepresentationofthecurrentsituationin
the environment that an agent finds itself in during a given discrete time step [3]. While distinctions can be
made with respect to whether an agent directly perceives raw state representations or only certain (partial)
observations thereof [17], this report will not distinguish these two cases, always assuming that an agent
has access to state representations fully describing current environmental states. Using its decision-making
strategy, i.e. policy, the agent selects in every state it encounters an action to be performed in the given
state [3]. Upon executing a selected action in a given state, the agent transitions into the succeeding state
and receives some numeric reward indicating how desirable taking the chosen action in the given state was
1https://github.com/openai/baselines/tree/master/baselines/ppo1
4

[3]. Each action may not only affect immediate rewards obtained by the agent, but potentially also future
rewards[3]. Inthisreport,itisassumedthatepisodes,orsequences,ofinteractionsbetweenanagentandits
environment, so-called trajectories, are always of a finite maximal length T. Training an RL agent involves
therepeatedapplicationoftwosuccessivetrainingsteps. Duringtheformertrainingstep, theagentismade
tointeractwithitsenvironmentforagivennumberoftrajectories. Relevantinformation,suchastransitions
fromone statetothe next, theactionschosenby theagent, aswellasthecorrespondingrewards emittedby
the environment, are recorded for the latter training step. In the latter training step, the agent’s policy gets
updated using information extracted from the data collected in the former training step. The goal of this
procedure is to update the agent’s policy so as to maximize the cumulative rewards that the agent expects
to receive from the environment over time given the agent’s choice of action in every environmental state
the agent encounters [3].
Formally,anRLagentissituatedinanenvironmentE,whichtheagentperceivesviastaterepresentations,
or states, s t , at discrete time steps t [3]. Each state s t ∈ S is drawn from a possibly infinite set of possible
states S [17], which describe the various environmental situations an agent might find itself in given the
agent’s environment E and its means of perceiving said environment. At any given time step t, an agent is
assumed to be situated in some perceived state s , where it has to choose which action a to perform. Upon
t t
performing action a t ∈ A in state s t , the agent transitions into the next state s t+1 ∈ S and receives some
reward r t ∈R [3, 4]. Here, A defines the action space, i.e. the set of all possible actions a t , which an agent
may choose to perform in a given state s . For the sake of simplicity, it is generally assumed that an action
t
space A remains constant across all possible states s t ∈S. Transitions from one state, s t , to the next, s t+1 ,
are assumed to happen stochastically and the sampling procedure can mathematically be denoted as s
t+1
P(s |s ,a ) [17]. The stochasticity involved in the sampling of next states is governed by a so-called
˜ t+1 t t
transitionprobabilitydistribution definedasP :S×A×S →R
≥0
[17]. Thistransitionprobabilitydistribution
determines for each possible state the conditional probability mass or density (depending on the nature of
the state space) values of being stochastically sampled as the next state given the current state, s , and the
t
action,a ,chosenbytheagentinthatstates [3]. Rewardsr areassignedtoanagentthroughsomereward
t t t
function definedasr :S×A →R[17]. Notethatalsoalternativedefinitionsofrewardfunctions,forexample
definitions involving stochasticity as presented in [3], are possible. Each initial state, to which an agent is
exposed immediately after the initialization of its environment, is stochastically drawn from the state space
S in accordance with a so-called initial state distribution, which is defined as ρ 1 :S →R ≥0 [17] and assigns
probability values to the elements in S, indicating how probable it is for any given state to be chosen as an
initial state. Concerning the notation of discrete time steps, note that this report always starts counting
time steps at t=1. Discrete time steps t in an agent’s environment thus range in [1,2,...,T]. Recall that T
denotes the finite maximal length of a given trajectory, and thus the finite maximal number of times that
an agent can transition from one state to the next by choosing some action in the former state. Speaking
about trajectories, those can more formally be defined as τ = (s ,a ,...,s ,a ,s ), where state s is
1 1 T T T+1 T+1
only observed as a final response to the T’th action, a , chosen in the T’th state, s , at time step t=T.
T T
The formalism described above yields the setup of a finite-horizon discounted Markov Decision Process
as long as the cumulative future rewards that the agent expects to receive over time by following its policy
are discounted by some discount factor γ ∈ (0,1] [17]. Dealing with a Markov Decision Process (MDP)
has the following implication: All relevant information an agent needs to have access to, in order to make
educated future predictions, must be determined by nothing but knowledge of the current state and the
possible actions that the agent might take in the current state. Thus, an agent is not required to have
memory capabilities concerning the past going beyond its knowledge of the current state. This follows from
the following fact. In an MDP, the probability of transitioning into some next state, s , and receiving the
t+1
correspondingreward,r ,whentransitioningintos ,mustdependonnothingbutthecurrentstates and
t t+1 t
the action a taken by the agent in s [18, 3]. Environmental states satisfying such a property are said to
t t
satisfy the Markov property [18, 3].
In order to predict actions, i.e. to map from states to actions, an RL agent employs a so-called policy,
whichimplementstheagent’sdecisionmakingstrategyandthusdeterminestheagent’sbehavior. Thepolicy
is iteratively updated during training of the agent. The exact methodology, how an agent maps from states
to actions, depends on the concrete RL algorithm being employed.
Whilesomepoliciesareinstantiatedasvaluefunctions(considerforexample[19])mappingonlyindirectly
5

from states to actions [20], other policies learn to directly predict actions a for every encountered state s
|     |     |     |     |     |     |     |     | t   | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
[20]. Policy gradient methods (PGMs) fall into the latter category [20, 4, 16]. In the subclass of PGMs
exclusively considered in this report, actions are stochastically sampled from some distribution over the
action space A, where the sampling is denoted as a π (a |s ), which means that action a is sampled from
|     |     |     |     |     |     | t˜ θ | t t | t   |     |
| --- | --- | --- | --- | --- | --- | ---- | --- | --- | --- |
with the conditional probability π (a |s ). Here, π refers to the agent’s policy, which is parameterized by
| A   |     |     |     | θ t | t   |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
theadjustable,i.e. trainable,parametersθ. Thecorrespondingdistribution,withrespecttowhichanaction
gets sampled, is parameterized as a deterministic function of an observed state s and the momentary state
t
of the policy’s trainable parameters θ. Since PPO is based on the principles of PGMs, PGMs will be further
elaborated on in Section 2.2. For the sake of completeness, note that other subclasses of PGMs exist where
actions are determined fully deterministically [20]. However, these methods will not be addressed in this
| report, | since they | are not relevant |     | to the | main content | of  | this report. |     |     |
| ------- | ---------- | ---------------- | --- | ------ | ------------ | --- | ------------ | --- | --- |
While the value function based RL algorithm described in [19] and policy gradient methods (PGMs),
like those described in [6, 16], are model-free in that they do not learn to estimate any explicit world model,
there are model-based RL algorithms where training the policy involves estimating an explicit model of the
| agent’s | environment | [3, 5]. |     |     |     |     |     |     |     |
| ------- | ----------- | ------- | --- | --- | --- | --- | --- | --- | --- |
Regardlessofanagent’sconcretewayofmappingfromstatestoactionsusingitspolicy,thegoalofevery
RLagentistolearnapolicywhichmaximizestheexpectedcumulativediscountedfuturereward,orexpected
E[R
return, ], which the agent expects to obtain following its policy π [4, 3]. More concretely, the expected
t
returnexpressesanagent’sexpectationonhowmuchrewardstheagentwillaccumulateoverthecourseofits
currenttrajectorywhencurrentlybeinginsomestates t andfollowingitspolicyπ,whereimmediaterewards
are weighted more strongly than rewards expected to be received more distant in the future. Formally, as
stated in [2], the return R , of which the agent seeks to maximize the expectation, can be defined as:
t
T
(cid:88)
|     |     |     |     |     | R = | γk−tr | ,   |     | (1) |
| --- | --- | --- | --- | --- | --- | ----- | --- | --- | --- |
|     |     |     |     |     | t   |       | k   |     |     |
k=t
where again T specifies maximal length of a trajectory, t the current time step for which the future reward
| is evaluated, | and γ | the discount | factor | mentioned |     | above. |     |     |     |
| ------------- | ----- | ------------ | ------ | --------- | --- | ------ | --- | --- | --- |
Being tightly connected to the concept of an expected return, further important concepts in RL are the
so-calledstateactionvalues,Q(s ,a ),andstatevalues,V(s ),aslearnedbycorrespondingstateactionvalue
|     |     |     | t   | t   |     |     | t   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
functions and state value functions, respectively. The state action value, or Q-value, specifies the expected
expected return R of taking action a in state s under the current policy and is defined as [4]:
|     |     | t   |     | t   | t      |       |       |     |     |
| --- | --- | --- | --- | --- | ------ | ----- | ----- | --- | --- |
|     |     |     |     |     | Q(s ,a | )=E[R | |s ,a | ],  | (2) |
|     |     |     |     |     | t      | t     | t t t |     |     |
whereEisthemathematicalexpectationoperator. ThestatevaluespecifiestheexpectedreturnR ofbeing
t
| in some | state s , following | the | current | policy, | and | is defined | as  | [4]: |     |
| ------- | ------------------- | --- | ------- | ------- | --- | ---------- | --- | ---- | --- |
t
|     |     |     |     |     | V(s | )=E[R | |s ]. |     | (3) |
| --- | --- | --- | --- | --- | --- | ----- | ----- | --- | --- |
|     |     |     |     |     |     | t     | t t   |     |     |
Those value functions are employed, among others, in value-based RL approaches [19], as well as in
| so-called | Actor-Critic | RL approaches, |     | of  | which PPO | is an | instance | [6]. |     |
| --------- | ------------ | -------------- | --- | --- | --------- | ----- | -------- | ---- | --- |
In the Actor-Critic approach, two functions get trained concurrently. The first function, being the so-
called critic, learns to approximate some value function [21]. This approximation is often an estimate of the
statevaluefunctionintroduced inEquation3[4]. Thesecond function, being theso-called actor, isapolicy,
which directly maps from states to actions as explained for PGMs above [21]. In the actor-critic approach,
the actor uses the value estimates produced by the critic when updating its trainable parameters [21], as
will be explained in more detail in Section 2.2. Training the actor utilizing the value estimates produced by
the critic is supposed to reduce the variance contained in the numeric estimates produced by some objective
function, based on which the actor gets trained [4, 21]. This reduction of variance, to be achieved when
training both an actor and a critic jointly, may lead to faster convergence of an RL algorithm compared to
other PGM training procedures not employing a critic when training the actor, i.e. the actual policy [21].
| Section | 2.2 will provide | further | information |     | on this. |     |     |     |     |
| ------- | ---------------- | ------- | ----------- | --- | -------- | --- | --- | --- | --- |
6

An important step in the development of the field of RL was the transition from traditional Reinforce-
ment Learning to Deep Reinforcement Learning (DRL). In DRL, functions to be learned by an agent get
approximated using artificial Neural Networks (NNs) or Recurrent NNs (RNNs), i.e. function approxima-
torsordynamicalsystemapproximators,respectively. WhenNNsorRNNsaretrainedtoapproximatesome
policyorvalueestimates, Stochastic Gradient Descent (SGD)[22]isusedtolearndirectmappingsfromraw
state representations to either actions or value estimates, depending on the type of policy or function being
approximated [2, 23]. When training DRL agents based on RNNs, naturally also the RNN’s hidden state is
taken into consideration when predicting the next action or value estimate. More on DRL will be explained
in Section 2.2. Also, since DRL has become the predominant approach to RL, from here on this report will
treat the two terms RL and DRL synonymously, with both terms jointly referring to DRL. Note that all
the formalisms mentioned above apply to both RL and DRL. Only DRL methods will be considered in the
remainder of this report.
AnotherimportantdimensionalongwhichRLapproachesgetdistinguishediswhethertheyareon-policy
or off-policy methods [24, 25]. In the former case, a RL agent’s policy or value function gets updated using
data exclusively generated by the current state of an agent’s policy, while off-policy methods may even use
training data having been generated using earlier versions of the current policy [24]. Note that Proximal
Policy Optimization (PPO) is an on-policy method that tends to stretch the notion of traditional on-policy
methods, since it uses the same training data generated by the current state of an agent’s policy for per-
forming multiple epochs of weight updates on the freshly sampled training data.
Lastly, it is worth mentioning that one formally distinguishes between RL approaches, where an agent
has access either to full state representations or, alternatively, to only certain observations thereof [17]. In
the latter case, an RL agent would not face a MDP, but a Partially Observable Markov Decision Process
(POMDP)[17]. InordertoavoidfurthercomplicationsoftheRLframeworkoutlinedsofar, thisreportwill
treat states and their observations as synonymous throughout, thereby only considering the case where an
agent’s interaction with its environment can be formalized as a MDP.
2.2 Policy Gradient Methods
Policy Gradient Methods (PGMs) are a class of RL algorithms, where actions are either directly and de-
terministically computed by some function or, alternatively, sampled stochastically with respect to some
probability distribution being defined over a given action space and parameterized via some deterministic
function. Those deterministic functions are functions of an agent’s currently experienced state [20] and
their trainable parameters are trained using stochastic gradient ascent (SGA) [26, 20] so as to maximize the
expected return E[R ] [20, 4]. This report will exclusively focus on PGMs using stochastic policies, where
t
actions are sampled stochastically, as this technique is used in PPO as well.
A popular subclass of PGMs implementing stochastic policies is the REINFORCE [16] family of RL
algorithms. REINFORCE algorithms sample actions a t ∈ A stochastically from the action space A in
accordancewithaprobabilitydistributioncomputedovertheactionspaceA [16],whereeachavailableaction
receives acertain probabilityof beingselected. Such adistribution isparameterized througha deterministic
function of the state s the agent currently faces [16]. Specifically, an action a is sampled with probability
t t
π (a |s ) [16, 20], where s denotes the state the agent experiences at the discrete time step t in the current
θ t t t
trajectory and π denotes an agent’s policy, being characterized by both a set of trainable parameters θ
θ
and a type of probability distribution used for sampling a . Adopting Mnih et al.’s [4] view on policies, a
t
policy is a mapping from states to actions. In accordance with this view, a policy may be thought of as a
processingpipelinerequiredtotransformgivenstaterepresentationss intocorrespondingactionsa . Thus,
t t
in REINFORCE algorithms a policy may be said to consist of three parts. The first part is a deterministic
function, assumed to be a NN or RNN in the present report, needed to transform a state representation s
t
into some parameterization. The second part is a generic probability distribution of a certain type to be
parameterized by the output of the former function. The third part is a succeeding random sampler used to
stochastically sample action a in accordance with the aforementioned distribution from the previous part.
t
In the following, the fundamentals of training PGMs using the aforementioned type of policies will be
explained. Since PPO is an instance of a REINFORCE algorithm, as will be explained in the next section,
7

the following explanations will only be concerned with the explanation of the fundamentals of how PGMs
| belonging |     | to the subclass | of  | REINFORCE |     | algorithms |     | are trained. |     |     |     |     |
| --------- | --- | --------------- | --- | --------- | --- | ---------- | --- | ------------ | --- | --- | --- | --- |
When training an RL agent employing a NN or RNN, the need arises for an objective function with
| respect | to  | which the | NN’s | or RNN’s | trainable |     | parameters | θ can | be updated. |     |     |     |
| ------- | --- | --------- | ---- | -------- | --------- | --- | ---------- | ----- | ----------- | --- | --- | --- |
In REINFORCE, as well as in PGMs in general [20], the objective one intends to optimize the policy’s
E[R
trainableparametersθ foristheexpectedreturn, ][4]. Thisquantityistobemaximizedbyperforming
t
| gradient |     | ascent on it | [4]. |     |     |     |     |     |     |     |     |     |
| -------- | --- | ------------ | ---- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Since the true expectation of the return R t is not available in practice in most cases, the expectation
of this value has to be approximated based on a finite number of samples when training an RL agent in
practice. InRL,theapproximationEˆ[·]ofanexpectedvalueE[·]isachievedbyaveragingthetheexpression
·containedintheexpectationoperatorEoveraso-calledminibatchofpreviouslycollectedtrainingdata[6].
The setup described above, combining gradient ascent with the use of minibatches, inevitably leads to
the use of SGA as an optimization procedure when training a REINFORCE algorithm whose trainable
parameters constitute a NN or RNN. Thereby, one hopes to move the trainable parameters to a location in
the parameter space, which is approximately at least a local optimum in maximizing the expected return.
Eˆ[R
Note, however, that the estimator of the expected return, t ], is treated as a constant when being
differentiatedwithrespecttothepolicy’strainableparametersθ. Therefore,directlycomputingthegradient
estimator ∇ Eˆ[R ] would yield the trivial zero vector. In order update the trainable parameters into a non-
|     |     | θ t |     |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
zero direction, the trainable parameters of a REINFORCE algorithm may be updated into the direction
∇ log π (a |s ) R , which is an unbiased estimate of the expected return [4, 16]. Thus, one may use the
| θ         |     | θ t t t            |     |      |          |           |       |             |            |     |     |     |
| --------- | --- | ------------------ | --- | ---- | -------- | --------- | ----- | ----------- | ---------- | --- | --- | --- |
| following |     | gradient estimator |     | when | training | REINFORCE |       | algorithms  | [4]:       |     |     |     |
|           |     |                    |     |      |          | gPG =Eˆ[∇ |       |             |            |     |     |     |
|           |     |                    |     |      |          |           | θ log | π θ (a t |s | t ) R t ], |     |     | (4) |
Eˆ[·]
where refers to an empirically estimated expectation (again being estimated by averaging over multiple
minibatchexamples)andlogπ (a |s )referstothelogprobabilityofselectingactiona afterhavingobserved
|     |     |     |     | θ   | t t |     |     |     |     | t   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
state s under the current policy π . In order to reduce the variance of the policy gradient estimator gPG,
|     | t   |     |     |     | θ   |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
one can opt for subtracting a baseline estimate b from the expected return R in Equation 4, leading to the
|           |     |                  |     |           |     |       | t        |            |           | t   |     |     |
| --------- | --- | ---------------- | --- | --------- | --- | ----- | -------- | ---------- | --------- | --- | --- | --- |
| following |     | variance reduced |     | estimator | of  | ∇ E[R | ], which | is defined | as [4]:   |     |     |     |
|           |     |                  |     |           |     | θ     | t        |            |           |     |     |     |
|           |     |                  |     |           | gVR | =Eˆ[∇ | log π    | (a |s )    | (R −b )]. |     |     | (5) |
|           |     |                  |     |           |     |       | θ        | θ t t      | t t       |     |     |     |
Note that also ∇ log π (a |s ) (R −b ) is an unbiased estimate of ∇ E[R ] [4, 16]. In practice, one may
|     |     | θ   | θ   | t t | t   | t   |     |     | θ   | t   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
choose the the state value function V(s t ) (see Equation 3) as a baseline b t , i.e. b t ≈ V(s t ) [4]. In such a
case, nowadays one would choose to approximate V(s ) by a NN or RNN. Furthermore, R can be seen as
|     |     |     |     |     |     |     |     | t   |     |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
an estimate of the state-action value function Q(s ,a ) (see Equation 2) associated with taking action a
|     |     |     |     |     |     |     | t   | t   |     |     |     | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
in state s [4], from which R results. Thus, R ≈ Q(s ,a ), which can in practice be estimated from the
|     |     | t   |     | t   |     |     | t   | t t |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
environmental responses observed after having executed the actions a chosen by the policy over the course
t
of some trajectory in the agent’s environment. Since R t −b t ≈ Q(s t ,a t )−V(s t ), the term R t −b t from
Equation 5 is nowadays often replaced in the literature by the so-called advantage estimate, or advantage in
| short, | being | defined | as [4]: |     |     |     |           |         |      |     |     |     |
| ------ | ----- | ------- | ------- | --- | --- | --- | --------- | ------- | ---- | --- | --- | --- |
|        |       |         |         |     |     | A t | =Q(s t ,a | t )−V(s | t ). |     |     | (6) |
Intuitively, A expresses how much better or worse it was to perform action a in state s , of which the
|     |     | t   |     |     |     |     |     |     |     | t   | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
quality is measured by R ≈ Q(s ,a ), compared to the value V(s ) one expected to receive for being in
|     |     |     | t   |     | t t |     |     |     | t   |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
state s while following the current policy π . Using the definition of A provided in Equation 6, this leads
|     | t   |     |     |     |     | θ   |     |     | t   |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
to the following policy gradient estimator nowadays often being used in practice [6]:
|     |     |     |     |     |     | gA =Eˆ[∇ | log | π (a |s | ) A ]. |     |     | (7) |
| --- | --- | --- | --- | --- | --- | -------- | --- | ------- | ------ | --- | --- | --- |
|     |     |     |     |     |     |          | θ   | θ t     | t t    |     |     |     |
Given this choice of policy gradient estimator, gA, one can consider REINFORCE agents as actor-critic
architectures (as introduced in Section 2.1), where the actor is the policy π and the critic is the state value
θ
network V comparing its state value predictions to the state-action values observed after having executed
| the | actions | predicted | by the | actor | [4]. |     |     |     |     |     |     |     |
| --- | ------- | --------- | ------ | ----- | ---- | --- | --- | --- | --- | --- | --- | --- |
The trainable parameters of an REINFORCE agent’s policy are then trained by performing SGA on a
| policy | gradient | estimator | such | as  | gA [4, | 16]. |     |     |     |     |     |     |
| ------ | -------- | --------- | ---- | --- | ------ | ---- | --- | --- | --- | --- | --- | --- |
8

Toolargeupdatesofapolicy’strainableparametersθcarrytheriskofmovingtheparametervectoraway
fromalocationassociatedwithalocalmaximumintheobjectivefunction’sperformancelandscapethatthe
parameter vector would ideally converge to when being repeatedly updated by small steps in the directions
of estimated policy gradients. Therefore, too large parameter updates must be avoided when updating a
policy’s trainable parameters θ.
InthecontextofPPOtraining,onespeaksofdestructively large policy updates whenreferringtoupdates
of the policy’s trainable parameters θ that are large enough to move the parameter vector θ away from
some local optimum [6]. In PGMs, those destructively large parameter updates may arise from performing
multiple epochs of parameter updates on the same set of freshly collected training data [6]. These are the
kinds of parameter updates that PPO tries to avoid by using a special objective function [6], as will be
explained throughout Section 3.
Speakingaboutepochs,notethatinthepresentreportanepoch ofparameterupdatesreferstoasequence
ofparameterupdatesbasedonSGA(orSGD),whereeachtrainingexamplefromtheavailablesetoftraining
data has been part of exactly one minibatch based on which one update of the trainable parameters has
been performed. Thus, an epoch refers to a sequence of weight updates resulting from a single pass through
the entire training data set.
3 Proximal Policy Optimization
Proximal Policy Optimization (PPO) is a Deep Reinforcement Learning (DRL) algorithm from the class of
policygradientmethods(PGMs)asintroducedinSection2.2[6]. Itstrainingprocedure,aswellasitsinput-
and output-behavior, largely follow that of standard REINFORCE algorithms. Thus, in order to map from
states to actions, a PPO agent uses a stochastic policy as introduced and explained in Section 2.2. More
on that will be explained in Section 3.2. In general, while PPO satisfies almost all conditions necessary
to be called a REINFORCE algorithm, there is one aspect where PPO does not exactly follow the general
definition of a REINFORCE algorithm or a PGM. This difference is due to the fact that PPO does not
always, due to a special objective function, update its trainable parameters exactly into the same direction
as REINFORCE algorithms or PGMs do. However, since the authors of PPO still call PPO a PGM, it
seems justified to likewise call PPO a member of the REINFORCE family of algorithms. Moreover, PPO is
an on-policy algorithm (see Section 2.1), since it exclusively uses the most recently collected training data
when performing one or multiple epochs of weight updates on its trainable parameters [6]. Also, PPO is a
model-free DRL algorithm.
Ashinteduponabove, themainfeaturedistinguishingPPOfromvanillaPGMs, includingREINFORCE
algorithms, is PPO’s particular objective function used for optimizing the algorithm’s trainable parameters,
i.e. weights and biases. Recall from Section 2.1 that on-policy PGMs, of which PPO is an instance, may
be criticized for being too sample-inefficient [6, 24, 20], meaning that these methods commonly use possibly
expensive to obtain training data only once for performing updates of their trainable parameters before
discarding the data in favor of newer data. PPO aims for improving upon PGMs’ sample efficiency by
employinganobjectivefunction, whichisparticularlydesignedtoallowformultipleepochsofupdatesofits
trainable parameters based on the same training data, as will be further elaborated on below.
Notethatatrustregion-basedPGMattainingcomparabledataefficiencyandreliableperformance,called
TrustRegionPolicyOptimization(TRPO)[27],hadbeenintroducedinthepastalready[6]. However,TRPO
suffered from several problems, which PPO tries to provide a solution for. Firstly, PPO is designed to use
a computationally cheaper update procedure for its trainable parameters compared to that used in TRPO
[6]. Secondly, PPO has been designed to be compatible with techniques like dropout or parameter sharing
(duringtraining), whileTRPOisnotcompatiblewiththesetechniques[6]. Thirdly, PPOhasbeendesigned
to be conceptually less complicated than TRPO [6].
Whilst being rather sub-optimally documented in the literature, even years after its invention PPO is
still a state-of-the-art DRL algorithm [15]. Therefore, in the following, the PPO algorithm will be explained
in thorough detail for the first time to the best of the author’s knowledge.
Section 3.1 will give an overview of the PPO algorithm, while Section 3.2 will describe PPO’s input-
9

output behavior, i.e. the way how actions are generated in response to observed states. How various target
valueestimatesarecomputedthroughouttrainingwillbedescribedinSection3.3. PPO’smaindemarcating
feature, its objective function, will be explained in minute detail in Section 3.4, followed by an explanation
in Section 3.5 of exploration strategies employed by PPO. Section 3.6 will deal with the question how to
back-propagate PPO’s overall objective function. Finally, PPO’s pseudocode is provided in Section 3.7.
3.1 Overview
PPO is a DRL algorithm, which is capable of learning to map state representations, or states, onto one or
multiple actions to be performed in in every observed state. Consider a PPO agent whose task it is to map
an observed state s t ∈ S onto a single action a t ∈ A to be performed in state s t . If multiple, independent
actions were to be performed in parallel by an agent in a given state s at time step t, those could be
t
indexed, using superscript (i), as a(i). Here, variable t ∈ 1,2,...,T again refers to a discrete time step in
t
a given trajectory of length T (see Section 2.1). S and A refer to the state and action spaces (see Section
2.1), respectively. Action spaces may either be continuous or discrete. Upon executing action a in state
t
s , the agent transitions into the next state state s and receives a reward r depending on the agent’s
t t+1 t
choice of action in state s [17]. Recall that actions are selected in given states by means of a policy π ,
t θ
where θ denotes policy’s trainable parameters. In PPO, θ refers to the trainable parameters, i.e. weights
and biases, of an artificial Neural Network (NN) or a Recurrent NN (RNN) [6]. Furthermore, since PPO is
a policy gradient method using a stochastic policy, the NN or RNN inside PPO’s policy is used to generate
the parameterization for some probability distribution with respect to which an action is sampled. Action
a is selected with probability π (a |s ) in state s given the policy’s current set of trainable parameters θ
t θ t t t
[6, 27, 20]. During training, the set of trainable parameters, θ, is repeatedly updated in incremental steps,
using stochastic gradient ascent (SGA), in a way such that an approximation of the expected return E[R ]
t
(see Equation 1) gets maximized [20].
In more detail, generating an action a from a given state s using policy π progresses in three con-
t t θ
secutive steps in PPO, which are executed in two separate portions of policy π . The first portion of π
θ θ
is the deterministic portion of the policy, which may be denoted as π , whereas the second portion of π
θd θ
is stochastic and can be denoted as π . In PPO, an action a is stochastically sampled from an agent’s
θs t
continuous or discrete action space A in accordance with a likewise continuous or discrete probability dis-
tribution defined over the PPO agent’s action space A, as will be further described in Section 3.2. Such
a distribution is generated and sampled from in the stochastic portion of the policy π , i.e. in π . The
θ θs
corresponding parameterization, here denoted as φ , for defining said probability distribution is calculated
t
in the deterministic portion of π , i.e. in π . In practice, π is a NN or RNN being parameterized by
θ θd θd
θ. Therefore, the deterministic portion of the policy, π , may also be referred to as policy network. For
θd
calculatingtheaforementionedparameterizationφ , thepolicynetworkπ consumesstates andcomputes
t θd t
the aforementioned parameterization φ in its output layer. If a PPO agent has to perform multiple actions
t
a(i) in parallel during each time step t, each action a(i) for i ∈ {1,2,...,I} is sampled from a respective
t t
action space A(i) with respect to a respective probability distribution δ(i) parameterized by a respective
t
parameterization φ(i). Each parameterization φ(i) is computed by a respective set of output nodes in the
t t
policy network’s output layer and any possible covariance between actions to be predicted in parallel during
a single time step is assumed to be zero, as will be explained in more detail for continuous action spaces in
Section3.2.1. Hence,eachactiona(i) isassumedtobestatisticallyindependentoftheotheractionssampled
t
during the same time step t.
Tothelevelofdetaildescribedabove, thisprocedureofsamplingactionsperfectlyfollowsthatdescribed
for the REINFORCE family of DRL algorithms [16], as described in Section 2.2. When explaining PPO’s
procedure for generating actions in more detail in Section 3.2, however, some aspect will be pointed out dis-
tinguishingPPO’sprocedureforsamplingactionsfromthatusedbytheREINFORCEfamilyofalgorithms.
While PPO’s main objective is to train an agent’s (deterministic) policy network π , training an agent
θd
actually involves training two networks concurrently [6], as described above already in the context of how
REINFORCE algorithms are trained (see Section 2.2). The first network is the policy network π itself,
θd
10

while the second network is a state value network V used to reduce the variance contained in the numeric
ω
estimates based on which the policy network is trained [4]. Here, ω denotes the trainable parameters of an
employed state value network V. Parameter sharing between π and V may apply [6, 4].
θ ω
TrainingaPPOagentmeansrepeatedlyalternatingbetweenthetwostepsofcollectingnewtrainingdata
and then updating both the policy network and state value network based on the freshly sampled training
dataformultipleepochs[6]. Theformerofthetwoaforementionedstepsmaybereferredtoasdatacollection
step or training data generation step, while the latter may be referred to as update step.
For the data collection step, N PPO agents in parallel are placed in separate, independent instances of
thesametypeofenvironment[6]. Then, whileallagentsareruninparallel, eachoftheN agentsismadeto
interactwithitsrespectiveenvironmentforT timesteps[6]. ItisnoteworthythatallN agentsusethemost
up-to-datestateofthepolicynetwork,whichisheldfixedduringadatacollectionstep[6]. IfeitheroftheN
parallelagentsencountersaterminalstatebeforeT timestepshaveelapsed,theenvironmentinstancesofall
N agents get reinitialized to new stochastically chosen initial states. Then, the agents continue interacting
withtheirenvironments. ThisprocessisrepeateduntileachagenthasexperiencedT timestepsduringadata
collection step [6]. Parallelizing agents’ interactions with their environments, as well as the reinitializations
of their environments, is done for the sake of efficiency. In this way, not only the interactions of the N
agents with their environments can be efficiently parallelized, but also the computations of the target values
Vtarget and A , which will be introduced below, can be largely parallelized using tensor operations. For
t t
each experienced state transition, the corresponding state s , next state s , action a , the corresponding
t t+1 t
probability of selecting action a in s given π , denoted as π (a |s ), and the corresponding reward r get
t t θ θold t t t
stored in a so-called tuple of the form o = (s ,a ,π (a |s ),s ,r ) in preparation for the next training
t t t θold t t t+1 t
step, i.e. the update step of the trainable parameter. Note that it would be more accurate to index each
observation tuple o by the two additional subscripts n and j, resulting in notation o . Here, n would
t t,n,j
indicate the index of the nth parallel agent that has generated a given observation tuple o . Subscript j
t
would indicate how many times an environment’s discrete time index t has been reset already due to an
environment’s reinitialization while generating the nth agent’s T state transition observations. However, for
simplicity of notation, we will generally omit explicitly stating the two subscripts n and j (unless stated
otherwise) and only implicitly assume the two subscripts n and j to be known for every tuple o .
t
For each observed state transition, encoded as a tuple of stored data o = (s ,a ,π (a |s ),s ,r ),
t t t θold t t t+1 t
two additional target values get computed and appended to the tuple. The first target value to be added
per tuple is target state value Vtarget associated with state s taken from a given observation tuple o . The
t t t
second target value to be added per tuple is the advantage A associated with having performed action a
t t
in state s [6, 4]. How Vtarget and A are concretely computed, using the state value network V , will be
t t t ω
described in Sections 3.3.1 and 3.3.2, respectively.
After training data has been collected, in the next step, i.e. the successive update step, the trainable
parametersofboththepolicynetworkπ andthestatevaluenetworkV getupdatedusingmultipleepochs
θd ω
of minibatch training on the freshly collected training data.
The clipped objective function used by PPO to train the policy network has specifically been designed
with the intention to avoid destructively large weight updates of the policy network while performing mul-
tiple epochs of weight updates using the same freshly collected training data. As indicated earlier, this is
meant to increase PPO’s data efficiency compared to that of other PGMs. The training data used here are
exclusively the observation tuples o = (s ,a ,π (a |s ),s ,r ,Vtarget,A ) collected during the immedi-
t t t θold t t t+1 t t t
ately preceding data collection step. The corresponding objective function, LCLIP, is defined as follows:
LCLIP(θ)=Eˆ [min(p (θ)A ,clip(p (θ),1−(cid:15),1+(cid:15))A )], (8)
t t t t t
whereEˆ [·]denotestheempiricalexpectationoperator,whichcomputesthevalueofitscontainedfunction
t
·astheaverageoverafinitesetoftrainingexamplescontainedinaminibatchoftrainingexamples[6]. Note
that each training example is one of the previously collected observation tuples o (or, more precisely, o
t t,n,j
using the extended notation as explained above). Unless indicated otherwise, in the context of performing
11

updates of the trainable parameters, i.e. during an update step, subscript t usually denotes the index of
a randomly sampled training example (taken from a minibatch of training examples), while it indicates,
duringadatacollectionstep,thediscretetimestepinsideagivenenvironmentduringwhichtheinformation
contained in an observation tuple has been observed. Minibatches of training data are sampled at random,
without replacement, from the pool of available observation tuples o . The term p (θ) in Equation 8 refers
t t
to a probability ratio being defined as:
π (a |s )
p (θ)= θ t t , (9)
t π (a |s )
θold t t
whereπ (a |s )denotestheprobabilityofactiona instates duringthedatacollectionstep(recordedin
θold t t t t
theobservationtuples),whileπ (a |s )referstotheprobabilityofa ins giventhemostup-to-datestateof
θ t t t t
π [6,27]. TheexpressionmininEquation8referstothemathematicaloperatorthatreturnstheminimum
θ
of its two input values and the clip operator clips its input value p (θ) to the value range [1−(cid:15),1+(cid:15)], where
t
(cid:15) is a hyperparameter that has to be chosen by the experimenter. For example, (cid:15)=0.2 according to [6].
The meaning of the individual terms contained in the objective function LCLIP, as well as the explana-
tion why this objective function is meant to allow for performing multiple epochs of weight updates based
on the same data without experiencing destructively large weight updates, will be explained in Section 3.4.
In theory, the policy network’s objective function LCLIP (Equation 8) is optimized, i.e. maximized,
usingstochasticgradientascent. Inpractice,however,commonlySGDisusedtominimize−LCLIP,thereby
treating the objective function to be maximized [6] as a loss function to be minimized. This is because stan-
darddeeplearninglibraries,usedtotrainDRLagentsinpractice,nowadaysonlysupportSGD,butnotSGA.
In order to encourage exploratory behavior of the policy network π during training, an Entropy bonus
θd
H can be added to the policy network’s objective function LCLIP, as will be further explained in Section
3.5. In theory, this works for both continuous and discrete action spaces A. However, as will be pointed
out later, this procedure is ineffective in encouraging exploration in continuous action spaces when strictly
following the PPO training procedure. Introducing a weighting factor h for the Entropy bonus, to control
the contribution of the Entropy bonus to the overall objective, and adding the weighted Entropy bonus to
the clipped objective function results in the objective function LEXPLORE =LCLIP +hH, which is then to
be maximized using stochastic gradient ascent. Again, when using stochastic gradient descent (as done in
practice), −LEXPLORE is to be minimized.
Training the state value network V is done via minimizing the squared error, averaged over multiple
ω
training examples contained in a minibatch, between a predicted state value V (s ) and the corresponding
ω t
targetstatevalueVtarget (seeSection3.3.1below)[6]. Thecorrespondinglossfunctionfortrainingthestate
t
value network V is therefore defined as:
ω
LV =Eˆ [(V (s )−Vtarget)2], (10)
t ω t t
where t refers to the index of some training example. The calculation of Vtarget will be described in Sec-
t
tion 3.3.1.
Finally, also taking the training of V into consideration, this results in the overall objective function
ω
LCLIP+H+V =LCLIP +hH −vLV (11)
to be optimized, i.e. maximized, using stochastic gradient ascent [6]. Just like scalar h, also v is a weighting
factor in Equation 11 to be chosen by the experimenter. When using stochastic gradient descent, again
−LCLIP+H+V has to be minimized. How this objective function is evaluated and back-propagated will be
explainedinSection3.6. Commonly,theAdamoptimizerisusedtoperformSGDtominimize−LCLIP+H+V
[6].
12

3.2 Generation of Actions
PPO is very flexible when it comes to its input-output behavior, i.e. the nature of the inputs and outputs
the algorithmcan learn to mapbetween. Upon receiving a singlestate representation s as input, the policy
t
generates as output one or multiple continuous or discrete actions to be executed in state s , depending on
t
therequirementsimposedbytheenvironment. Here,theonlyrestrictionthatappliesisthatboththeinputs
and outputs must be scalar or multi-dimensional real numbers. Unless indicated otherwise, for the sake if
simplicity, we will generally assume that only a single a action a is generated per time step t, and thus per
t
state representation s , by an agent’s policy.
t
As stated in Section 3.1, PPO maps states to actions using a stochastic policy π , i.e. each action a is
θ t
(pseudo-)randomly sampled from the action space A with respect to some probability distribution δ t com-
puted over A. Such a probability distribution δ
t
is parameterized by a set of parameters denoted φ
t
. Such
a parameterization φ can be computed either partially or entirely as a function of an input state s . In
t t
the literature (see [16] or Sections 2 and 3 of the supplementary material associated with [4]) it has been
suggested to compute all components of φ as a function of an input state s using NNs or RNNs. This
t t
is what is commonly done in REINFORCE algorithms. As an exception to this, in the case of generating
continuous actions, the inventors of PPO [6] made use of a technique originally proposed in [27], where
parameterization φ is only partially defined as a function of state representation s , while some component
t t
of φ is independent of s . This will be further explained in Section 3.2.1.
t t
More concretely, in the most generic way, the generation of actions in PPO can be described as follows.
First, a PPO agent receives a state representation s . This state representation is passed through the policy
t
network, i.e. the NN or RNN constituting the deterministic portion of the agent’s policy π . This results
θ
in (at least a part of) a set of parameters φ being computed in the policy network’s output layer. The
t
parameterization φ may have to be post-processed in order to normalize probability mass estimates or to
t
enforce a non-negative standard deviation. The potentially post-processed set of parameters is then used to
parameterize a probability distribution δ
t
, which is defined over the agent’s action space A. Finally, action
a t is sampled from action space A in accordance with δ t by applying a random sampler to δ t .
This procedure can easily be generalized to the case where multiple, say I, actions a(i) (where index
t
i ∈ {1,2,...,I}) have to be generated and executed concurrently each time step t, i.e. in each state s . In
t
this case, the policy network features I sets of output nodes, where each set of output nodes produces (at
leastasubsetof)onesetofparametersφ(i) usedtoparameterizeasingleprobabilitydistributionδ(i). Here,
t t
probability distribution δ(i) is defined over action space A(i). Note that each individual parameterization
t
φ(i) mayhavetobepost-processedasdescribedabove. Finally,foreachprobabilitydistributionδ(i),asingle
t t
action a(i) ∈A(i) is sampled.
t
In order for a PPO agent to be able to deal with state representations of different nature, the network
architecture of the policy network may be varied. If state representations are of visual nature, the policy
network’s input layers may be convolutional NN layers. Otherwise, they may be fully-connected NN layers.
Sections3.2.1and3.2.2describetheparticularproceduresforgeneratingcontinuousanddiscreteactions,
respectively.
3.2.1 Continuous Action Spaces
In [6], the inventors of PPO propose to use a technique for generating continuous actions, which has previ-
ously been proposed in [27]. In the following, this technique will be described.
In PPO, scalar continuous actions are stochastically sampled from one-dimensional Gaussian distribu-
tions. Thus, a N(µ ,σ ). Such a Gaussian distribution N(µ ,σ ) is denoted δ and parameterized by a
t ˜ t t t t t
set of parameters φ = {µ ,σ }, where µ and σ are the mean and the standard deviation of the Gaussian
t t t t t
distributionδ ,respectively. Themeanµ iscomputedasafunctionofstaterepresentations experiencedat
t t t
time step t in a given trajectory. As proposed in [27], the standard deviation σ , controlling the exploratory
t
13

behavior of an agent during training, is determined independently of state representation s .
t
In more detail, generating a continuous action works as follows. When a PPO agent receives a state
representations ,s isfedthroughthepolicynetwork,i.e. thedeterministicportionofthepolicy. Theoutput
t t
node of the policy network computes the mean µ , which is used to parameterize a Gaussian distribution
t
δ
t
defined over the continuous action space A. The standard deviation σ
t
used to parameterize δ
t
is a
hyperparametertobechosenbytheexperimenterbeforetheonsetoftrainingandiskeptfixedthroughoutthe
entire training procedure. Then, a random sampler is applied to the parameterized probability distribution
δ and action a gets sampled by the random sampler with respect to δ . More on the choice of the standard
t t t
deviation hyperparameter σ will be explained in Section 4.2.
t
How to generalize this procedure to multiple action spaces has been explained in the introduction of
Section 3.2. Particularly, in this case, I output nodes of the policy network generate I means µ(i) used
t
to parameterize I statistically independent Gaussian distributions δ(i). The I standard deviations used to
t
parameterize the I probability distributions δ(i) are all taken to be the same fixed constant. The remaining
t
procedureisdescribedabove. Note,thatwhenpredictingmultiplecontinuousactions,onemayalternatively
and analogously sample an I-dimensional point from a I-dimensional Gaussian distribution featuring an
I-dimensional mean vector, consisting of the I outputs generated by the policy network, and a diagonal
covariance matrix containing the fixed standard deviation parameter along its diagonal. In this case, the
value along the ith dimension of a point sampled from the I-dimensional Gaussian distribution constitutes
action a(i).
t
3.2.2 Discrete Action Spaces
In [6], the inventors of PPO propose to use a technique for generating discrete actions, which has previously
been proposed in [4]. In the following, this technique will be described.
InPPO,discreteactionsaredrawnfromacorrespondingdiscreteactionspacedefinedasA ={1,2,...,M}.
Here,M denotesthenumberofavailableoptionstodrawanactiona from. Eachnaturalnumbercontained
t
in action space A, i.e. 1 through M, is representative one action available in the agent’s environment. An
action a t is sampled from action space A with respect to a Multinomial probability distribution δ t being
defined over A. The probability distribution δ t assigns each available element in A a probability of being
sampled as action a . The probability distribution’s parameterization is denoted φ and computed by the
t t
policy network as a function of a received state representation s .
t
More concretely, this works as follows. Upon receiving a state representation s , s is fed through
t t
the policy network. The policy network’s output layer contains a set of nodes yielding the unnormalized
probability mass estimates for choosing either of the possible actions in A as a t . Precisely, the mth output
node yields the unnormalized probability of sampling the mth element from action space A as action a t .
Applying the Softmax [28] function to the outputs generated by the policy network yields a vector φ of
t
normalized probability mass estimates, where the mth element of vector φ , φ , specifies the normalized
t t,m
probabilityofsamplingthemth elementfromA asa
t
givenareceivedstaterepresentations
t
. Thevectorφ
t
isusedtoparameterizeaMultinomialprobabilitydistributionδ . Finally,actiona isobtainedbyapplyinga
t t
random sampler to probability distribution δ , where action a is sampled with probability φ =π (a |s ).
t t t,at θ t t
Howthisprocedurecanbegeneralizedtoproducemultipleactionsa(i) concurrentlyineverystates has
t t
been described in the introduction of Section 3.2.
3.3 Computation of Target Values
As explained in Section 3.1, training a PPO agent involves computing target state values Vtarget and ad-
t
vantage estimates A . The technique used to compute those quantities uses so-called n-step returns [4], as
t
previouslypresentedin[18,29]accordingto[4], andisparticularlysuitablefortrainingRNN-basednetwork
architectures [6]. Note that target state values Vtarget and advantage estimates A are always calculated
t t
during the training data generation steps of a PPO agent’s training procedure [6]. That means that target
values are always computed based on the states of the policy network and the state-value network during
the most recent training data generation step [6]. Those states of the policy and the state value network
14

aredenotedπ andV , respectively, todistinguishthemfromthecorrespondingstatesπ andV being
|                    | θold ωold |              |     |               |       |     |     | θ   | ω   |
| ------------------ | --------- | ------------ | --- | ------------- | ----- | --- | --- | --- | --- |
| repeatedly updated | during    | a successive |     | weight update | step. |     |     |     |     |
According to [6], an alternative method for calculating target state values and advantages estimates in
the context of training PPO agents, which is not considered here, is presented in [30].
Sections 3.3.1 and 3.3.2 describe for target state values and advantage estimates respectively what these
| quantities measure | and how      | they | are computed. |     |     |     |     |     |     |
| ------------------ | ------------ | ---- | ------------- | --- | --- | --- | --- | --- | --- |
| 3.3.1 Target       | State Values |      |               |     |     |     |     |     |     |
Vtarget
Following a concept presented in [4], a target state value denotes the discounted cumulative reward
t
associatedwithhavingtakenanactiona inagivenstates andiscomputedbasedontheobservationsmade
|     |     |     |     | t   | t   |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
throughout an experienced trajectory, i.e. a sequence of interactions between an agent and its environment.
InthecontextoftrainingaPPOagent,targetstatevaluesVtarget
|     |     |     |     |     |     |     | areusedintwoways. | Firstly,theyare |     |
| --- | --- | --- | --- | --- | --- | --- | ----------------- | --------------- | --- |
t
used to train the state value network V (see Equation 10). Secondly, they are used to compute advantage
ω
estimates A t for training the policy network (see Section 3.3.2). They are computed as follows.
First, an agent is made to interact with its environment for a given maximal trajectory length, i.e. a
maximal number of time steps T (as described in Section 3.1). When a given trajectory ends, target state
valuesarecomputedforeverystates experiencedduringthetrajectoryaccordingtothefollowingequation:
t
Vtarget
|     |     | =r +γr |     | +γ2r | +...+γn−1r |       | +γnV (s | ),  | (12) |
| --- | --- | ------ | --- | ---- | ---------- | ----- | ------- | --- | ---- |
|     | t   | t      | t+1 | t+2  |            | t+n−1 | ωold    | t+n |      |
whereVtarget denotesthetargetstatevalueassociatedwithagivenstate(orstaterepresentation)s ,r is
| t   |     |     |     |     |     |     |     |     | t t |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
the reward received for having executed action a in state s , and γ ∈(0,1] is the discount factor mentioned
|     |     |     |     | t   | t   |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
earlier. Furthermore, the term t+n denotes the time step at which the trajectory under consideration
terminated in the final state s t+n . If the trajectory terminated due to the maximal trajectory length T
being reached, V (s ) denotes the state value associated with state s as predicted by the state value
|     | ωold t+n |     |     |     |     |     | t+n |     |     |
| --- | -------- | --- | --- | --- | --- | --- | --- | --- | --- |
network. Otherwise, V (s ) is set to 0, since this condition indicates that the agent reached a terminal
|     | ωold | t+n |     |     |     |     |     |     |     |
| --- | ---- | --- | --- | --- | --- | --- | --- | --- | --- |
state within its environment from where onward no future rewards could be accumulated any longer. Since
target state values are computed during a data collection steps, i.e. before the onset of a training iteration’s
weight update step, the state of the state value network used to compute Vtarget is denoted V rather
|     |     |     |     |     |     |     | t   | ωold |     |
| --- | --- | --- | --- | --- | --- | --- | --- | ---- | --- |
than V .
ω
Vtarget,
By using the aforementioned way of computing target state values each observed reward is used
t
to compute up to T target state values [4]. Thereby, this procedure potentially increases the efficiency of
propagating the information contained in each observed reward to the corresponding value estimates being
| dependent on    | it [4].   |     |     |     |     |     |     |     |     |
| --------------- | --------- | --- | --- | --- | --- | --- | --- | --- | --- |
| 3.3.2 Advantage | Estimates |     |     |     |     |     |     |     |     |
Following [4, 6], an advantage estimate A t quantifies how much better or worse the observed outcome of
choosingacertainactioninagivenstatewascomparedtothestate’sestimatedvaluepredictedbythestate
value network. Here, the qualitative outcome of choosing an action a in a given state s , being compared
|     |     |     |     |     |     |     | t   | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Vtarget
to a state’s predicted value, is measured by the state’s target state value (see Section 3.3.1). Thus,
t
the computation of advantage estimates extends the computation of target state values described above.
The equation for calculating an advantage estimate A , associated with having taken action a in state
|                  |        |                 |     |            | t      |             |     | t   |     |
| ---------------- | ------ | --------------- | --- | ---------- | ------ | ----------- | --- | --- | --- |
| s as experienced | during | some trajectory |     | of maximal | length | T, is given | by: |     |     |
t
=Vtarget−V
|     |     |     |     | A t | ωold | (s t ), |     |     | (13) |
| --- | --- | --- | --- | --- | ---- | ------- | --- | --- | ---- |
t
Vtarget
where the target state value is computed as described in Section 3.3.1 [6, 4] and V (s ) refers to
|     |     | t   |     |     |     |     |     | ωold t |     |
| --- | --- | --- | --- | --- | --- | --- | --- | ------ | --- |
the state value, associated with state s , predicted by the state value network during the data generation
t
step.
Intuitively, Equation 13 makes sense, since it compares the observed return Vtarget, associated with
t
having taken action a in state s , to the currently estimated value V (s ) of state s . It is a practical
|     | t   |     | t   |     |     |     | ωold t | t   |     |
| --- | --- | --- | --- | --- | --- | --- | ------ | --- | --- |
15

implementation of the more theoretical equation for calculating advantage estimates A provided in Equa-
t
tion 6 above.
NotethatthereisatypoinEquation10in[6],whichconcernsthecomputationoftheadvantageestimate
A inPPO.InthatEquation, thetermγT−t+1 ismeanttobeγT−t−1 accordingtothelogicoftheequation,
t
since otherwise the reward r gets discounted disproportionately strongly.
T−1
3.4 Explanation of Policy Network’s Main Objective Function LCLIP
This Subsection will explain in detail the main objective function, LCLIP, which is used to update PPO’s
policynetworkusingstochsticgradientascent(SGA).AsstatedinEquation8,theobjectivefunctionLCLIP
isdefinedasLCLIP(θ)=Eˆ [min(p (θ)A ,clip(p (θ),1−(cid:15),1+(cid:15))A )],wherep (θ)isaprobabilityratiobeing
t t t t t t
defined as p (θ)= πθ(at|st) [6] and A , as introduced in Section 2.2, is an advantage estimate associated
t πθold (at|st) t
with taking an action a in state s . How to concretely compute A has been described in Section 3.3.2
t t t
above. Since the topic treated here is concerned with performing updates of an employed NN’s or RNN’s
trainable parameters, subscript t denotes the index of a training example contained in a randomly sampled
minibatch again. Again, π (a |s ) refers to the probability of choosing action a in state s given the most
θ t t t t
up-to-date state of the policy network, while π (a |s ) refers to the probability of choosing action a in
θold t t t
state s given the state of the policy network during the most recent training data generation step.
t
This Subsection will be structured as follows. Firstly, a motivation for using LCLIP will be given. Af-
terwards, the function’s constituting components will be explained individually, finally leading up to the
description of how the objective function LCLIP overall behaves under different conditions concerning the
values of its input arguments. Throughout, the focus of this subsection will not primarily lie on how the
objective function behaves in the forward pass, but rather on the more important and more involved topic
of how it behaves in the back-propagation pass.
As stated in Section 2.2, nowadays many DRL algorithms from the class of policy gradient methods
(PGMs) are trained by performing SGA on the policy gradient estimator gA =Eˆ[∇ log π (a |s ) A ] (see
θ θ t t t
Equation7). This,however,asindicatedearlier,usuallydoesnotallowforusingtrainingdataefficiently[6].
This is because updating the policy repeatedly, i.e. for multiple epochs, based on the same freshly collected
trainingdatamayleadtodestructivelylargeweightupdates[6]asthedifferencebetweentheoldstateofthe
policy, used for generating the training data, and the updated state of the policy increases with the number
of weight updates performed.
To facilitate more efficient usage of the training data, the main objective function employed by PPO,
LCLIP, is particularly designed to allow for multiple epochs of weight updates on the same set of training
data. In order to allow for multiple epochs of weight updates on the same data, PPO’s main objective
function,LCLIP,aimsroughlyspeakingatlimitingtheextenttowhichthecurrentstateofthepolicycanbe
changed compared to the old state used for collecting the training data. More precisely, it aims at limiting
to which extent the policy can be changed even further through consecutive weight updates after a rough
approximation of the divergence between the updated state of the policy and the old state moves beyond a
given threshold value while performing multiple epochs of weight updates on the same training data.
This idea of limiting the impact of weight updates on the state of the policy network had already been
explored in Trust Region Policy Optimization (TRPO) [27]. However, one of the downsides associated with
using TRPO is its computationally relatively expensive and inflexible procedure used for limiting the extent
to which weight updates may change its policy’s trainable parameters θ [6]. Therefore, the inventors of
PPO aimed at using a more inexpensive to evaluate, more flexible, and conceptually simpler approximation
of divergence between the two policy states π and π based on which they could limit the impact of
θ θold
individualweightupdatesonthepolicyπ . Asaconsequence,theinventorsofPPOoptedfordirectlyincor-
θ
porating a cheap, simple, and flexible to evaluate probability ratio into PPO’s main objective function (or
policy gradient estimator) LCLIP. This probability ratio acts as a measure, or rather rough approximation,
of divergence between π and π and is defined as p (θ)= πθ(at|st) . According to [6], the use of the
θ θold t πθold (at|st)
16

| probability | ratio | p (θ) | has originally | been | proposed | in [31]. |     |     |     |
| ----------- | ----- | ----- | -------------- | ---- | -------- | -------- | --- | --- | --- |
t
| The | behavior | of the | probability | ratio | p t (θ) | is as follows: |     |     |     |
| --- | -------- | ------ | ----------- | ----- | ------- | -------------- | --- | --- | --- |
If an action a becomes more unlikely under the current policy, π , (in a given environmental state s )
|     |     | t   |     |     |     |     |     | θ   | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
than it used to be under the old state of the policy, π , the probability ratio will vanish towards positive
θold
| 0, since | π (a | |s ) shrinks | compared | to  | π (a | |s ). |     |     |     |
| -------- | ---- | ------------ | -------- | --- | ---- | ----- | --- | --- | --- |
|          | θ t  | t            |          |     | θold | t t   |     |     |     |
When actions become more likely under the current state of the policy, π , than they used to be under
θ
the old one, π θold , the probability ratio p t (θ) will grow and approach positive infinity in the limit, since
| π (a |s | ) grows | compared | to π | (a |s | ).  |     |     |     |     |
| ------- | ------- | -------- | ---- | ----- | --- | --- | --- | --- | --- |
| θ t t   |         |          | θold | t     | t   |     |     |     |     |
In cases where the probability of choosing some action a in a given state s is comparatively similar
t t
under both the current and the old state of the policy, i.e. when the behavior of both states of the policy is
similar in terms of the given metric, the probability ratio will evaluate to values close to 1. This is because
| the two | probabilities |     | π (a |s ) | and π | (a |s | ) will have similar | values. |     |     |
| ------- | ------------- | --- | --------- | ----- | ----- | ------------------- | ------- | --- | --- |
|         |               |     | θ t t     | θold  | t t   |                     |         |     |     |
Thus, again, the probability ratio p (θ) can be seen as a cheap to evaluate measure, or rough indicator,
t
of divergence between the two states π θ and π θold of the policy. Note that this divergence measure does not
compute any established metric to accurately assess how different the two states of the policy are across all
possible actions in all possible states. Instead, it assesses for each training example in a given minibatch
how much the behavior of the policy has changed with respect to the training example currently under
consideration.
WheninspectingLCLIP,onecanseethattheprobabilityratiop (θ)isusedintwoplaces. Thefirstterm
t
the probability ratio is part of is the so-called unclipped objective [6], being defined as p (θ)A . The second
t t
term that the probability ratio is part of, clip(p (θ),1−(cid:15),1+(cid:15))A , is referred to as clipped objective [6].
|     |     |     |     |     |     | t   |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Clipping, as done by the clipping operator clip, here refers to the act of restricting the range of values, that
LCLIP
p t (θ) can take, to the interval [1−(cid:15),1+(cid:15)]. The minimum operator, min, in takes the minimum of
the unclipped and clipped objective and returns it as the result of evaluating LCLIP.
This design of LCLIP is supposed to have two effects. Firstly, it is supposed to yield a pessimistic, i.e.
lower, estimate of the policy’s performance [6]. Secondly, it is supposed to avoid destructively large weight
updates into the direction increasing the probability of re-selecting some action a in a given state s [6]. To
t t
seehowthisworks,boththeclippingoperatorandtheminimumoperatorhavetobeexplainedinmoredetail.
The first operator to be inspected in more detail is a clipping operator, clip, which is part of the afore-
mentionedclippedobjectiveandensuresthatitsfirstinputargument, theprobabilityratiop t (θ), lieswithin
a specific interval defined by the operator’s second and third input arguments, namely 1−(cid:15) and 1+(cid:15).
| Mathematically, |     | the clipping | operation |     | is defined | as follows: |     |     |     |
| --------------- | --- | ------------ | --------- | --- | ---------- | ----------- | --- | --- | --- |

|     |     |     |     |     |     | 1−(cid:15) | if  | p (θ)<1−(cid:15) |     |
| --- | --- | --- | --- | --- | --- | ---------- | --- | ---------------- | --- |
|     |     |     |     |     |     |           |     | t                |     |
clip(p (θ), 1−(cid:15), 1+(cid:15))= 1+(cid:15) if p (θ)>1+(cid:15) (14)
|     |     |     |     | t   |     |     |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
p
|     |     |     |     |     |     | t (θ) |     | else |     |
| --- | --- | --- | --- | --- | --- | ----- | --- | ---- | --- |
Clipping, i.e. restricting, the range of values that the probability ratio p (θ) can take is supposed to
t
remove the incentive for pushing the probability ratio outside the interval enforced by the clipping operator
during repeated updateson the same data [6]. Put differently, due to the clipping operation, the probability
ratiop (θ)issupposedtoremainwithintheinterval[1−(cid:15),1+(cid:15)]evenaftermultipleepochsofweightupdates
t
performed on the same data. Thereby, the goal of avoiding destructively large weight updates is supposed
to be achieved.
In order to see why this is supposed to be the case, consider the following. When a clipping operator’s
input value to be clipped falls outside the interval of admissible input values, i.e. when clipping applies, the
partial derivative leading through the clipping operator becomes 0. Particularly, here the partial derivative
of the clipping operator’s output value with respect to the clipping operator’s input value p t (θ) becomes 0
if clipping applies [32]. This is due to the fact that the clipping operator’s output value is constant when
clippingapplies. Constants,inturn,evaluateto0whenperformingdifferentiationonthem. Onlywhenclip-
ping does not apply, i.e. when 1−(cid:15)≤p (θ)≤1+(cid:15), the partial derivative of the clipping operator’s output
t
valuewithrespecttotheinputvaluetobeclippedis1andthereforenon-zero(seeEquation20includingthe
17

corresponding explanation). Due to the nature of the back-propagation algorithm, only the zero-gradient
will result from back-propagation paths leading through operators inside an objective function, whose par-
tial derivatives are 0. Therefore, only the zero-gradient will result from the back-propagation paths leading
through the clipping operator when clipping applies during a corresponding forward-pass. Thus, training
examples, where the only non-zero gradient component is back-propagated via the clipping operator, will
not cause a resulting gradient to point into a direction in parameter space, where the probabilities of some
action in a given state will change even more extremely if it has changed enough already for clipping to
apply. Thereby, multiple epochs of weight updates may safely be performed on the same training data (at
least in theory; a critical assessment of that claim will be provided in Section 5.1).
ThesecondoperatorofimportanceinLCLIP tobeexplainedinmoredetailisthemathematicalminimum
operator,min,whichreturnstheminimumofitstwoinputarguments. Recallthattheminimumoperatoris
employedinLCLIP toreturntheminimumoftheunclippedandclippedobjective. Thepartialderivativeof
the minimum operator’s output value with respect to its minimal input value is 1, while the partial deriva-
tive of the output value with respect to the other input value is 0 (see Equations 21 and 22). Note that,
mathematically speaking, the minimum operator is not differentiable when both its input arguments are
equivalent. In practice, however, machine learning packages like PyTorch do select either of two equivalent
input values as the minimum input value, thereby causing the minimum operator to be differentiable even
when this would mathematically speaking not be the possible.
Given all the background knowledge stated above, below it will be explained case-wise how PPO’s main
objective function LCLIP behaves when systematically varying its input arguments’ values. Readers only
being interested in a short summary of the explanations presented below may skip to Table 1 summarizing
the contents presented in the remainder of this subsection.
In cases where clipping does not apply, i.e. in cases where the probability ratio lies within the interval
[1−(cid:15),1+(cid:15)], neither the clipping operator nor the minimum operator impact the computation of the gradi-
ent. Instead, thegradientassociatedwithatrainingexamplewhereclippingdoesnotapplywillpointintoa
direction locally maximizing the unclipped objective p (θ)A . This is irrespective of whether the advantage
t t
estimate A , introduced in Section 2.2, is positive or negative.
t
Next, consider the cases where the probability ratio p (θ) is lower than the threshold value 1−(cid:15), i.e.
t
p (θ) < 1−(cid:15). In that case, clipping applies and the behavior of LCLIP depends on whether the advantage
t
estimate A is positive or negative.
t
Iftheadvantageestimateispositive,i.e. A >0,theminimumoperatorwillreceiveasitsinputarguments
t
arelativelysmallpositivevaluefortheunclippedobjective,p (θ)A ,andalargerpositivevaluefortheclipped
t t
objective, clip(p (θ),1−(cid:15),1+(cid:15))A . In this case, where A > 0 and p (θ) < 1−(cid:15), the minimum operator
t t t t
will be dominated by the smaller unclipped objective p (θ)A . Intuitively, this means the following. If the
t t
probability of selecting some action a in a state s has decreased during the previous weight updates, as
t t
indicated by p (θ)<1−(cid:15), but choosing a in s was better than expected, as indicated by A >0, then the
t t t t
gradient will point into the direction maximizing p (θ)A . Thus, the training example in question will try to
t t
influence the gradient, which is computed over a minibatch of training examples, in such a way that action
a becomes more likely in state s again.
t t
If the advantage estimate is negative, i.e. A < 0, while p (θ) < 1−(cid:15), then the behavior of LCLIP
t t
changes. In this case, multiplying a negative advantage estimate A by a small positive value for p (θ),
t t
being smaller than 1−(cid:15), will result in a negative value of less magnitude (in the negative direction) than is
obtained by multiplying the negative value A by the corresponding clipped probability ratio evaluating to
t
at least 1−(cid:15). In this case, where A < 0 and p (θ) < 1−(cid:15), the minimum operator will return the clipped
t t
objective,evaluatingtothenegativevalue(1−(cid:15))A . Thus,clippingapplieswhenA <0whilep (θ)<1−(cid:15).
t t t
Since clipping applies, the gradient associated with a training example satisfying the aforementioned condi-
tions will be the zero-gradient. As a consequence, a corresponding training example will not encourage the
gradient, which is computed over an entire minibatch of training examples, to point into a direction making
the probability of selecting action a in state s , being associated with a negative advantage A , even more
t t t
unlikelyifithasbecomeunlikelyenoughalreadyfortheprobabilityratiotodropbelow1−(cid:15). Ifthiswasnot
18

the case, destructively large weight updates could result due to the increasingly larger divergence between
the two states of the policy indicated by the probability ratio considerably diverging from value 1 already.
Now, consider the two cases where p (θ)>1+(cid:15). Also here, the behavior of LCLIP depends on whether
t
| the advantage | estimate | A is positive | or negative. |     |     |     |     |
| ------------- | -------- | ------------- | ------------ | --- | --- | --- | --- |
t
Consider the case where A >0 while p (θ)>1+(cid:15). In this case, the probability of choosing an action a
|     |     | t   | t   |     |     |     | t   |
| --- | --- | --- | --- | --- | --- | --- | --- |
associated with a positive advantage estimate A in state s has become considerably larger already under
t t
the current policy than it used to be under the old state of the policy. This is indicated by the condition
p (θ) > 1+(cid:15). In this case, clipping applies and the minimum operator will return the clipped objective as
t
its minimal input value. Thus, since the overall objective value is clipped, only the zero-gradient will result
from a training example where A t > 0 while p t (θ) > 1+(cid:15). Also in this case, destructively large weight
updates are supposed to be prevented through the resulting zero-gradient, as explained above already.
Lastly, consider the case where the advantage estimate is negative, i.e. A < 0, while p (θ) > 1+(cid:15).
|     |     |     |     |     | t   | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
In such a case, the probability of selecting an action a in a state s has become considerably larger under
|     |     |     |     | t   | t   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
the current state of the policy than it used to be under the old state, while choosing action a in state
t
s t led to a worse outcome than expected, as indicated by the negative advantage estimate A t . Here, the
clipped objective will evaluate to the negative value (1+(cid:15))A , while the unclipped objective will evaluate
t
to a negative value p (θ)A of magnitude larger than (1+(cid:15))A . Consequently, the minimum operator will
|     | t   | t   |     | t   |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
return the unclipped objective, p (θ)A , being of larger magnitude in the negative direction. Therefore, a
|     |     | t   | t   |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
training example satisfying the conditions A < 0 and p (θ) > 1+(cid:15) will be associated with a non-zero
|     |     |     | t   | t   |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
gradient pointing into the direction maximizing the negative value p t (θ)A t . This is to rigorously correct the
behavior of the policy in the case that an action has become more likely in a given state, potentially as a
byproductofupdatingthepolicyonothertrainingexamples,eventhoughpastexperiencehasindicatedthat
thechosenactionwasworsethanexpectedinthegivenstateintermsoftheexperiencedadvantageestimate.
Here, no means of preventing destructively large weight updates applies. Also, in this way, the objective
LCLIP
function aims at yielding a pessimistic estimate of the policy’s performance. Drastic contributions
to the objective value are only admissible if they make the valuation of the objective worse, but are clipped,
i.e. bounded, when they would lead to an improvement of the objective value [6].
|     |                               |     | Return Value | Objective  | Sign of   |          |     |
| --- | ----------------------------- | --- | ------------ | ---------- | --------- | -------- | --- |
|     | p (θ) >0                      | A   |              |            |           | Gradient |     |
|     | t                             |     | t of min     | is Clipped | Objective |          |     |
|     | p (θ)∈[1−(cid:15),1+(cid:15)] | +   | p (θ)A       | no         | +         | (cid:88) |     |
|     | t                             |     | t t          |            |           |          |     |
(cid:88)
|     | p t (θ)∈[1−(cid:15),1+(cid:15)] | −   | p t (θ)A t      | no  | −   |          |     |
| --- | ------------------------------- | --- | --------------- | --- | --- | -------- | --- |
|     | p (θ)<1−(cid:15)                | +   | p (θ)A          | no  | +   | (cid:88) |     |
|     | t                               |     | t t             |     |     |          |     |
|     | p t (θ)<1−(cid:15)              | −   | (1−(cid:15))A t | yes | −   | 0        |     |
|     | p (θ)>1+(cid:15)                | +   | (1+(cid:15))A   | yes | +   | 0        |     |
|     | t                               |     | t               |     |     |          |     |
|     | p (θ)>1+(cid:15)                | −   | p (θ)A          | no  | −   | (cid:88) |     |
|     | t                               |     | t t             |     |     |          |     |
Table 1: Table summarizing the behavior of PPO’s objective function LCLIP for all non-trivial cases, where
bothp t (θ)andA t areunequalzero. Thefirstcolumnindicatesthevalueoftheprobabilityratiop t (θ), while
the second column indicates whether the advantage estimate A is positive (+) or negative (−) for a given
t
training example (indexed by subscript t) taken from a minibatch of training examples. The third column
indicatestheoutputofLCLIP,i.e. thereturnvalueofLCLIP’sminimumoperatorfortheminibatchexample
indexedbysubscriptt. Thefourthcolumnindicateswhetherthisterm,i.e. theoutputofLCLIP,isaclipped
ThefifthcolumnindicateswhetherthesignofthevaluereturnedbyLCLIP
| term(yes)ornot(no). |     |     |     |     |     | ispositive |     |
| ------------------- | --- | --- | --- | --- | --- | ---------- | --- |
(+)ornegative(−). Thelastcolumnindicateswhetherthegradientresultingfromback-propagatingLCLIP
aims at maximizing the value returned by LCLIP ((cid:88)) or whether only the trivial zero-gradient (0) results.
| 3.5 | Exploration | Strategies |     |     |     |     |     |
| --- | ----------- | ---------- | --- | --- | --- | --- | --- |
In DRL, the exploration-exploitation dilemma refers to the problem of balancing how much a learning agent
explores itsenvironmentbytaking novel actions inthe states itencountersand how much the agentchooses
19

to exploit the knowledge it has gained throughout training thus far already [7]. If the agent explores the
differenteffectsthatdifferentactionshaveingivenstates,theagentmightencountermorevaluablebehaviors
than it has previously found [7]. On the other hand, if the agent over-explores its environment, this might
leadtoslowconvergencetoanoptimalpolicy,eventhoughover-exploringagentsmightstillpossiblyconverge
tosomelocaloptimumintermsofanagent’slearnedpolicy[33]. Thus,goodexploration-exploitationtrade-
off strategies are very important in order to ensure that the agent will eventually converge, in reasonable
time, on some optimal policy after having sufficiently explored its state-action space in search for the most
| valuable | actions | in given | states. |     |     |     |     |     |     |     |
| -------- | ------- | -------- | ------- | --- | --- | --- | --- | --- | --- | --- |
In PPO (and more generally in all REINFORCE [16] algorithms), exploration is naturally incorporated
intothelearningprocedurebymeansofthestochasticpolicyπ ,whichstochasticallysamplesactionsinstead
θ
of computing them solely deterministically as a function of given states. In the following, Sections 3.5.1 and
3.5.2willdescribehowtheexploratorybehaviorofaPPOagent’sstochasticpolicyisregulatedforcontinuous
| and   | discrete    | action spaces, | respectively. |        |        |     |     |     |     |     |
| ----- | ----------- | -------------- | ------------- | ------ | ------ | --- | --- | --- | --- | --- |
| 3.5.1 | Exploration | in             | Continuous    | Action | Spaces |     |     |     |     |     |
Recall from Section 3.2.1 that continuous actions are sampled from Gaussian distributions in PPO. Each
Gaussianisparameterizedbyameanandastandarddeviation. Here,theonlywayofadjustingtheexpected
spread of values to be sampled around the mean of a Gaussian distribution is to adjust the Gaussian’s
standarddeviation. However,asstatedinSection3.2.1,inPPOthestandarddeviationofGaussiansisfixed
throughout the training procedure. Thus, in practice there is no way of adjusting the exploratory behavior
ofaPPOagentexceptforadjustingthefixedstandarddeviationparametermanuallybeforethestartofthe
training procedure.
An alternative procedure, treating a Gaussian’s standard deviation as a trainable parameter to be ad-
justed throughout training by means of stochastic gradient descent, is presented in [16].
| 3.5.2 | Exploration | in  | Discrete | Action Spaces |     |     |     |     |     |     |
| ----- | ----------- | --- | -------- | ------------- | --- | --- | --- | --- | --- | --- |
In the case of discrete action spaces, an Entropy bonus H can be added to the policy network’s overall
objective function in order to enhance a PPO agent’s exploratory behavior. Recall from Section 3.2.2 that,
in the case of discrete action spaces, an action a ∈A is sampled from a discrete action space A with respect
t
toaMultinomialprobabilitydistributionparameterizedbyaprobabilityvectorφ assigningaprobabilityof
t
beingsampledasactiona toeachoftheelementsinA. Insuchasituation,maximalexplorationisachieved
t
when assigning equal probability to each of the elements contained in A. An Entropy bonus rewards an
agent’s tendency to produce probability estimates over the action space A which make all available actions
equally likely in a given state, thereby leading to many distinct actions being explored in the states encoun-
| tered | throughout | training. |     |     |     |     |     |     |     |     |
| ----- | ---------- | --------- | --- | --- | --- | --- | --- | --- | --- | --- |
Using the notation introduced in this report, the Entropy of a Multinomial distribution parameterized
for a single training example (taken from a minibatch of training examples) is defined as:
|         |                   |     |     | M        | M        | n (cid:18) | n (cid:19) |     |               |          |
| ------- | ----------------- | --- | --- | -------- | -------- | ---------- | ---------- | --- | ------------- | -------- |
|         |                   |     |     | (cid:88) | (cid:88) | (cid:88)   | φqt,m(1−φ  |     |               |          |
| Hfull(φ | ,q ,n)=−log(n!)−n |     |     | φ log(φ  | )+       |            |            |     | )n−qt,m log(q | !). (15) |
|         | t t               |     |     | t,m      | t,m      | q          | t,m        | t,m |               | t,m      |
t,m
|     |     |     |     | m=1 | m=1qt,m=0 |     |     |     |     |     |
| --- | --- | --- | --- | --- | --------- | --- | --- | --- | --- | --- |
Recall that φ denotes a vector of normalized probability estimates (associated with a single training
t
example taken from a minibatch of training examples), where the vector’s mth element, φ , denotes the
t,m
probabilityofselectingtheactionspace’smth elementasactiona inagivenstates . Thevectorq contains
|     |     |     |     |     |     | t   |     |     | t   | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
the counts of how many times each element contained in action space A = {1,2,...,M} has been sampled
in a given state s as action a . This vector’s mth element, q , denotes how many times the mth element
|     |     | t   | t   |     |     | t,m |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
from hasbeen sampledinstate s . Sinceexactlyone actiona issampledinevery states , alwaysexactly
|     | A   |     |     | t   |     | t   |     |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
one element of q will be 1, while all other elements will be 0. The variable n is computed as the sum over
t
vector q , and thus counts how many actions are sampled in total in a given state. Therefore, always n=1.
t
|     | (cid:0) n | (cid:1) |     |     |     |     | (cid:0) n | (cid:1) | n!  |     |
| --- | --------- | ------- | --- | --- | --- | --- | --------- | ------- | --- | --- |
The term is the so-called binomial coefficient, being computed as = . Given that
|     | qt,m |     |     |     |     |     | qt,m | qt,m!(n−qt,m)! |     |     |
| --- | ---- | --- | --- | --- | --- | --- | ---- | -------------- | --- | --- |
20

always n = 1 and q ∈ {0,1}, it can be shown that the binomial coefficient will always evaluate to 1 in
t,m
|     |     |     |     |     |     | (cid:80)M (cid:80)n | (cid:0) n | (cid:1) φq t ,m(1−φ | )n−qt,m |     |
| --- | --- | --- | --- | --- | --- | ------------------- | --------- | ------------------- | ------- | --- |
the cases considered here. Likewise, here the term t, m t,m log(q t,m !) will
|     |     |     |     |     |     | m=1 | qt,m=0 qt | ,m  |     |     |
| --- | --- | --- | --- | --- | --- | --- | --------- | --- | --- | --- |
always evaluate to 0, since q ∈ {0,1}, so that always log(q !) = log(1) = 0. Furthermore, since always
|     |     |     | t,m |     |     |     | t,m |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
n = 1, also −log(n!) will always evaluate to 0. Thus, in the context of computing the Entropy bonus for a
Multinomial distribution used to sample a single action a per state s , Equation 15 can be simplified to:
|     |     |     |     |     |     | t   |     | t   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
M
(cid:88)
|     |     |     |     | HShannon(φ | )=− | φ   | log φ | ,   |     | (16) |
| --- | --- | --- | --- | ---------- | --- | --- | ----- | --- | --- | ---- |
|     |     |     |     |            | t   |     | t,m   | t,m |     |      |
m=1
which corresponds to the definition of the so-called Shannon Entropy. The evaluation of Equation 16,
yielding the Entropy bonus for a single training example, is then averaged over a minibatch of training
examples. This gives rise to the complete equation used for computing the Entropy bonus when training a
| PPO agent | on a | discrete action | space. | It looks | as  | follows: |     |     |     |     |
| --------- | ---- | --------------- | ------ | -------- | --- | -------- | --- | --- | --- | --- |
M
|     |     |     |     | H(φ)=Eˆ |     | (cid:88) |          |     |     |      |
| --- | --- | --- | --- | ------- | --- | -------- | -------- | --- | --- | ---- |
|     |     |     |     |         | [−  | φ        | log φ ]. |     |     | (17) |
|     |     |     |     |         | t   | t,m      | t,m      |     |     |      |
m=1
Eˆ
Here, denotes the empirical expectation again and φ denotes a minibatch of normalized probability
vectors φ . M refers to the number of elements, i.e. possible actions, in action space A and t is again used
t
to denote the index of a training example taken from a minibatch of training examples.
| 3.6 Back-Propagation |     |     | of  | Overall | Objective |     | Function |     |     |     |
| -------------------- | --- | --- | --- | ------- | --------- | --- | -------- | --- | --- | --- |
Recall that during the training of a PPO agent, both a stochastic policy network and a state value network
get trained, with the latter being used to reduce the variance contained in the numeric estimates based on
which the former gets trained [4]. Throughout Section 3, we have seen so far how the objective function
LCLIP+H+V = LCLIP +hH −vLV, presented in Equation 11 and being used to train a PPO agent in
its entirety, decomposes. While the clipped objective function LCLIP (see Equation 8) is used to train the
policy network, the quadratic loss LV (Equation 10) is used to train the state value network. In order to
encourageexplorationinthecaseofdiscreteactionspaces,anEntropybonusH (Equation17)canbeadded
| to the overall | objective. |     |     |     |     |     |     |     |     |     |
| -------------- | ---------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Next, we will consider how these separate terms are back-propagated by working out the partial deriva-
tives of the terms contained in the overall objective function with respect to the outputs produced by the
policy network and the state value network. To make the following more practically oriented, we will con-
sider the optimization procedure as one where a loss function has to be minimized. Thus, we will consider
the computation of partial derivatives from a perspective where −LCLIP+H+V has to be minimized using
| stochastic | gradient | descent. |     |     |     |     |     |     |     |     |
| ---------- | -------- | -------- | --- | --- | --- | --- | --- | --- | --- | --- |
Concretely, the remainder of this subsection is structured as follows. In Section 3.6.1, it will be shown
−LCLIP,
how to compute the partial derivative of the negative clipped objective function, with respect to
the probability value π (a |s ) serving as input to the computation of −LCLIP. This yields the definition
θ t t
∂−LCLIP
of the partial derivative . The result obtained from the aforementioned derivation applies to both
∂πθ(at|st)
continuous and discrete action spaces, since the procedure is identical in both cases. Subsection 3.6.2 will
showhowtocomputethepartialderivativeofπ θ (a t |s t )withrespecttotheoutputµ t computedbythepolicy
network in the case of continuous action spaces, yielding the definition of ∂πθ(at|st). Applying the chain rule
∂µt
|     |     | ∂−LCLIP |     | ∂πθ(at|st) |     | ∂−LCLIP |     |     |     |     |
| --- | --- | ------- | --- | ---------- | --- | ------- | --- | --- | --- | --- |
for differentiation to and yields for continuous action spaces. Subsection 3.6.3
|     |     | ∂πθ(at|st) |     | ∂µt |     | ∂µt |     |     |     |     |
| --- | --- | ---------- | --- | --- | --- | --- | --- | --- | --- | --- |
will show how to compute the partial derivative of π (a |s ) with respect to the outputs φ computed
|     |     |     |     |     |     | θ t | t   |     | t,m |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
by the policy network in the case of discrete action spaces, yielding the definition of the partial derivatives
∂πθ(at|st). Applying the chain rule to ∂−LCLIP and ∂πθ(at|st) yields ∂−LCLIP for discrete action spaces.
| ∂φt,m |     |     |     | ∂πθ(at|st) |     | ∂φt,m |     | ∂φt,m |     |     |
| ----- | --- | --- | --- | ---------- | --- | ----- | --- | ----- | --- | --- |
Subsection 3.6.4 will show how to compute the partial derivative of LV with respect to the output of the
state value network. Subsection 3.6.5 shows how to compute the partial derivative of the negative Entropy
bonus, −H, with respect to the normalized outputs produced by the policy network in the case of discrete
21

action spaces.
To keep computations more tractable, Sections 3.6.1 through 3.6.5 will consider how to compute the
aforementioned partial derivatives for only a single training example (taken from a minibatch of training
examples)atatime. Furthermore,inthefollowing,thelogarithmoperatorlogdenotesthenaturallogarithm.
3.6.1 Back-Propagation of LCLIP
ThisSubsectionwillshowhowtocomputethepartialderivativeofthelossfunction−LCLIP,i.e. −1∗LCLIP,
with respect to the probability value π (a |s ) serving as input to the evaluation of the aforementioned loss
θ t t
function.
Recall that LCLIP, here being shown as the loss function associated with a single training example
t
identified by subscript (i.e. index) t, is defined as follows:
LCLIP(θ)=min(p (θ)A ,clip(p (θ),1−(cid:15),1+(cid:15))A ), (18)
t t t t t
where p (θ) denotes the probability ratio being defined as p (θ)= πθ(at|st) .
t t πθold (at|st)
Equation 18 can be differentiated with respect to π (a |s ) as follows:
θ t t
∂−LCLIP ∂−LCLIP (cid:18) ∂LCLIP ∂p (θ)A ∂LCLIP ∂clip(p (θ))A ∂clip(p (θ)) (cid:19) ∂p (θ)
t = t t t t + t t t t t
∂π (a |s ) ∂LCLIP ∂p (θ)A ∂p (θ) ∂clip(p (θ))A ∂clip(p (θ)) ∂p (θ) ∂π (a |s )
θ t t t t t t t t t t θ t t
(19)
That is:
∂−LCLIP (cid:18)(cid:26) 1 if p (θ)A ≤clip(p (θ),1−(cid:15),1+(cid:15))A
t =−1∗ t t t t ∗A +
∂π (a |s ) 0 else t
θ t t
(cid:26)
1 if clip(p (θ),1−(cid:15),1+(cid:15))A <p (θ)A
t t t t ∗A ∗ (20)
0 else t
(cid:26) 1 if 1−(cid:15)(cid:54)p (θ)(cid:54)1+(cid:15) (cid:19) 1
t ∗
0 else π (a |s )
θold t t
Note that the definitions of Equations 19 and 20 imply that the partial derivatives of the minimum
operator min with respect to its two input arguments are defined as:
∂min(x,y) (cid:26) 1 if x(cid:54)y
= (21)
∂x 0 else
and
(cid:26)
∂min(x,y) 1 if y <x
= (22)
∂y 0 else
Also, Equations 19 and 20 imply that the partial derivative of the clipping operator clip with respect to its
input argument to be clipped is defined as:
∂clip(x,a,b) (cid:26) 1 if a(cid:54)x(cid:54)b
= (23)
∂x 0 else
Strictlymathematicallyspeaking,theminimumoperatorisnotdifferentialbewhenitsinputsxandy are
equivalent. Likewise,mathematicallyspeaking,theclippingoperatorisnotdifferentiablewhenitsfirstinput
argumentxisequivalenttotheinputsaorbdefiningtheboundarieswhereclippingapplies. However,tokeep
those operators differentiable in situations where their partial derivatives would not be defined otherwise,
deep learning software packages like PyTorch make use of the partial derivatives provided in Equations 21
through 23 rather than using those being strictly mathematically correct.
22

3.6.2 Continuing Back-Propagation of LCLIP in Continuous Action Spaces
In the following, it will be shown how to compute the partial derivative of the probability π (a |s ) with
|     |     |     |     |     |     |     |     |     |     | θ t | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
respect to the mean parameter µ computed by the policy network in the case of continuous action spaces.
t
Before stating the definition of ∂πθ(at|st), first note how the probability value π (a |s ) is obtained via a
θ t t
∂µt
forward-pass through the policy network. To obtain π (a |s ), the policy network has to be evaluated on a
|     |     |     |     |     |     |     | θ t t |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | ----- | --- | --- | --- | --- |
given state s , where s is provided by a training example, indexed by subscript t, taken from a minibatch
t t
of training examples. Thereby, the mean parameter µ is computed, which is then used to parameterize a
t
Gaussiandistribution. ModerndeeplearningsoftwarepackageslikePyTorchorTensorFlowsupportobtain-
ing the log-probability log π θ (a t |s t ) of selecting a given action a t in state s t by evaluating the Gaussian’s
|     |     |     |     |     | √1  | e−1 | (at − µt)2 |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | ---------- | --- | --- | --- | --- |
probability density function (PDF) g(a t )= 2 σ t , to which the logarithm log has to be applied,
|     |     |     |     |     | σt  | 2π  |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
on value a . Note that action a is also provided by the given training example and denotes the action that
|     | t   |     | t   |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
has been taken in state s during the previous training data generation step. Inserting log π (a |s ), i.e.
|            |          | t           |          |     |            |     |             |      |       | θ t | t   |
| ---------- | -------- | ----------- | -------- | --- | ---------- | --- | ----------- | ---- | ----- | --- | --- |
| log g(a ), | into the | exponential | function |     | exp yields | the | probability | π (a | |s ). |     |     |
| t          |          |             |          |     |            |     |             | θ    | t t   |     |     |
∂πθ(at|st)
Noticefromtheabovethatcomputingthepartialderivative involvesdifferentiatingaGaussian’s
∂µt
PDF,towhichthelogarithmhasbeenapplied,withrespecttomeanparameterµ . Thecorrespondingpartial
t
∂log g(at),
derivative which has previously been derived in [16], is defined as follows:
∂µt
|       |          |                |     | ∂log       | g(a ) | ∂log     | π (a |s ) | a −µ |     |     |      |
| ----- | -------- | -------------- | --- | ---------- | ----- | -------- | --------- | ---- | --- | --- | ---- |
|       |          |                |     |            | t =   |          | θ t t     | = t  | t.  |     | (24) |
|       |          |                |     | ∂µ         |       |          | ∂µ        | σ2   |     |     |      |
|       |          |                |     |            | t     |          | t         |      | t   |     |      |
| Using | Equation | 24, ∂πθ(at|st) |     | is defined | as    | follows: |           |      |     |     |      |
∂µt
|     |     |     | ∂π  | (a  | |s ) | ∂π (a  | |s ) ∂log | π (a | |s ) |     |      |
| --- | --- | --- | --- | --- | ---- | ------ | --------- | ---- | ---- | --- | ---- |
|     |     |     |     | θ t | t    | θ      | t t       | θ    | t t  |     |      |
|     |     |     |     |     | =    |        |           |      | ,    |     | (25) |
|     |     |     |     | ∂µ  |      | ∂log π | (a |s )   | ∂µ   |      |     |      |
|     |     |     |     | t   |      |        | θ t t     | t    |      |     |      |
∂πθ(at|st)
| where |     | =exp(log |     | π θ (a t |s | t )). More | concretely: |     |     |     |     |     |
| ----- | --- | -------- | --- | ----------- | ---------- | ----------- | --- | --- | --- | --- | --- |
∂log πθ(at|st)
|     |     |     |     | ∂π (a | |s )         |     |         | a     | −µ  |     |      |
| --- | --- | --- | --- | ----- | ------------ | --- | ------- | ----- | --- | --- | ---- |
|     |     |     |     | θ     | t t =exp(log |     | π (a |s | ))∗ t | t.  |     | (26) |
|     |     |     |     | ∂µ    |              |     | θ t     | t     | σ2  |     |      |
|     |     |     |     |       | t            |     |         |       | t   |     |      |
LCLIP
| 3.6.3 Continuing |     | Back-Propagation |     |     | of  |     | in Discrete | Spaces |     |     |     |
| ---------------- | --- | ---------------- | --- | --- | --- | --- | ----------- | ------ | --- | --- | --- |
In the following, it will be shown how to compute the partial derivatives of the probability π (a |s ) with
|     |     |     |     |     |     |     |     |     |     | θ t | t   |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
respect to the normalized probability mass estimates φ t,m computed by the policy network in the case of
| discrete action | spaces. |     |     |     |     |     |     |     |     |     |     |
| --------------- | ------- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Before stating the definition of the partial derivatives, first note how the probability value π (a |s ) is
θ t t
obtained via a forward-pass through the policy network. To obtain π (a |s ), the policy network has to be
|     |     |     |     |     |     |     |     |     | θ t t |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | ----- | --- | --- |
evaluatedonagivenstates providedbyatrainingexample,indexedbysubscriptt,takenfromaminibatch
t
of training examples. Thereby, the probability mass estimates φ t,1 through φ t,M are computed, which are
thenusedtoparameterizeaMultinomialdistribution. ModerndeeplearningsoftwarepackageslikePyTorch
and TensorFlow support obtaining the log-probability log π (a |s ) of selecting a given action a in state
|     |     |     |     |     |     |     | θ   | t t |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
s by evaluating the Multinomial probability distribution’s probability mass function (PMF), to which the
t
logarithm log has to be applied, on value a . Here, a refers to the action taken in state s during the
|     |     |     |     |     | t   |     | t   |     |     | t   |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
previous training data generation step. The probability π θ (a t |s t ) is obtained from log π θ (a t |s t ) by inserting
log π (a |s ) into the exponential function exp. Then, one has to compute the set of partial derivatives of
| θ t | t   |     |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
the probability π (a |s ) with respect to the probability mass estimates φ through φ computed in the
|     | θ   | t t |     |     |     |     |     |     | t,1 | t,M |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
forward pass. This involves computing the partial derivative of the Multinomial distribution’s PMF with
respect to the probability mass estimates φ through φ . Continuing to use the notation introduced in
|                |     |        |               |     | t,1          |     | t,M        |             |     |     |     |
| -------------- | --- | ------ | ------------- | --- | ------------ | --- | ---------- | ----------- | --- | --- | --- |
| Section 3.2.2, | the | PMF of | a Multinomial |     | distribution |     | is defined | as follows: |     |     |     |
n!
|     |     |     |     |     |       |         | φx1 ···φxM |     |     |     |      |
| --- | --- | --- | --- | --- | ----- | ------- | ---------- | --- | --- | --- | ---- |
|     |     |     |     |     | g(·)= |         |            | ,   |     |     | (27) |
|     |     |     |     |     |       | q !···q | ! t,1      | t,M |     |     |      |
|     |     |     |     |     |       | 1       | M          |     |     |     |      |
where n denotes the total number of actions sampled in each state s . This is always 1 here, since only
t
a single action a is sampled in each state s . Furthermore, q through q denote how often each of the M
|     | t   |     |     |     | t   |     |     | 1   | M   |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
23

elements in the discrete action space A has been sampled in state s t . The probabilities φ t,1 through φ t,M
indicate the probability of choosing the M elements contained in action space A respectively. Let φ t,at be
the probability of choosing action a in state s and let q = 1 indicate that action a has been sampled
t t at t
during the previous training data generation step in state s , while all remaining q through q are 0, since
t 1 M
neither of the other actions has been sampled in state s .
t
Simplifying Equation 27 based on the observations stated above yields:
g(·)=φx1 ···φ xat ···φxM =φ0 ···φ1 ···φ0 =1···φ ···1=φ , (28)
t,1 t,at t,M t,1 t,at t,M t,at t,at
since n! becomes 1! =1 and all q through q , except for q =1, are 0.
x1!···xat !···xM! 0!···1!···0! 1 M at
Applying the logarithm to Equation 28 yields:
log g(·)=log(φ ). (29)
t,at
Notethatφ =π (a |s )and,giventhesimplificationsdonetoEquation27,thatlogg(·)=logπ (a |s ).
t,at θ t t θ t t
Differentiating log g(·) from Equation 29 with respect to the probability mass estimate of sampling the
previously sampled action a results in:
t
∂log g(·) 1
= . (30)
∂φ φ
t,at t,at
Differentiatinglogg(·)withrespecttotheprobabilityofsamplinganyalternativevaluefromactionspace
A, which has not been sampled in state s
t
during the training data generation step, results in:
∂log g(·)
=0, (31)
∂φ
t,m(cid:54)=at
for all actions m∈A, where m(cid:54)=a
t
.
Finally, this results in the following partial derivatives:
∂π (a |s ) ∂π (a |s ) ∂log π (a |s ) 1
θ t t = θ t t θ t t =exp(log π (a |s ))∗ (32)
∂φ ∂log π (a |s ) ∂φ θ t t φ
t,at θ t t t,at t,at
and
∂π (a |s ) ∂π (a |s ) ∂log π (a |s )
θ t t = θ t t θ t t =exp(log π (a |s ))∗0=0 (33)
∂φ ∂log π (a |s ) ∂φ θ t t
t,m(cid:54)=at θ t t t,m(cid:54)=at
3.6.4 Back-Propagation of state-value Network’s Objective Function
The quadratic loss function, used to train the state value network on a a single training example indexed by
subscript t is defined as LV =(V (s )−Vtarget)2. The partial derivative of the quadratic loss function LV
t ω t t t
with respect to the output V (s ) generated by the state value network is defined as follows:
ω t
∂LV ∂(V (s )−Vtarget)2
t = ω t t =2∗(V (s )−Vtarget). (34)
∂V (s ) ∂V (s ) ω t t
ω t ω t
3.6.5 Back-Propagation Entropy Bonus in Discrete Action Spaces
In the following, it will be shown how to compute the partial derivative of the Entropy bonus in the case of
discreteactionspaceswithrespecttotheoutputsgeneratedbythepolicynetwork. Again,thecomputations
will be shown for a single training example, indexed by subscript t, at a time. Also, the Entropy bonus will
be treated as a loss term to be minimized again.
24

RecallthatcomputingtheEntropybonusforaMultinomialprobabilitydistributioninthecaseofdiscrete
action spaces was done by evaluating the Shannon Entropy, stated in Equation 16, on the probability mass
estimatesφ t,1 throughφ t,M producedbythepolicynetworkforagivenstates t . Howtocomputeφ t,1 through
φ has been explained in Section 3.6.3. Next, it will be shown how to compute the partial derivatives
t,M
of the negative Shannon Entropy, −H , with respect to the probability mass estimates φ through φ .
|               |      |              |     |         | t       |            |             |     | t,1 | t,M |
| ------------- | ---- | ------------ | --- | ------- | ------- | ---------- | ----------- | --- | --- | --- |
| First, recall | that | the negative |     | Shannon | Entropy | is defined | as follows: |     |     |     |
M
(cid:88)
|     |     |     | −H = | φ   | log φ | =φ log  | φ +···+φ | log φ . |     | (35) |
| --- | --- | --- | ---- | --- | ----- | ------- | -------- | ------- | --- | ---- |
|     |     |     | t    | t,m |       | t,m t,1 | t,1      | t,M t,M |     |      |
m=1
DifferentiatingEquation35withrespecttothenormalizedoutputgeneratedbythepolicynetwork’smth
| output node | yields: |     |     |     |     |     |     |     |     |     |
| ----------- | ------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
∂−H
|     |     |     |     |     |     | t =log | φ +1. |     |     | (36) |
| --- | --- | --- | --- | --- | --- | ------ | ----- | --- | --- | ---- |
t,m
|     |     |     |     |     | ∂φ  | t,m |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
3.7 Pseudocode
| Algorithm   | 1 shows | the | pseudocode     | of  | the overall | PPO algorithm. |     |     |     |     |
| ----------- | ------- | --- | -------------- | --- | ----------- | -------------- | --- | --- | --- | --- |
| 4 Reference |         |     | Implementation |     |             |                |     |     |     |     |
To facilitate the understanding of the PPO algorithm, which has only been explained theoretically so far
throughout this paper, a reference implementation has been produced. The reference implementation can
be found at https://github.com/Bick95/PPO. The main objective while writing the code has been to de-
liver an easy to understand, but consequently less rigorously efficient and competitive implementation. In
the following, the provided reference implementation will be introduced (Section 4.1) and a corresponding
| evaluation      | thereof | will | be presented | (Section |                | 4.2). |     |     |     |     |
| --------------- | ------- | ---- | ------------ | -------- | -------------- | ----- | --- | --- | --- | --- |
| 4.1 Description |         |      | of Reference |          | Implementation |       |     |     |     |     |
When designing the provided reference implementation, two competing objectives had to be balanced. As
indicated above, the main objective while writing the code has been to deliver an easy to read implemen-
tation to facilitate the reader’s understanding of the PPO algorithm. The second objective, central to the
development of the reference implementation, has been the implementation’s ease of use, which involved a
lot of added complexity to make it easy for an user to customize a PPO agent’s training procedure. To
balance those two aspects, the reference implementation has been designed in a very modular way. This
makes it easier to observe the implementation of isolated parts of the overall DRL algorithm, while, at the
same time, making it easy to exchange modules in order to adapt the implementation to different learning
conditions. A strategic design choice has been to keep the code immediately concerned with training a PPO
agent as clean and concise as possible, while outsourcing a lot of the involved complexity into separate parts
of the code.
The produced reference implementation has been developed in Python using the PyTorch library. The
implementation can be applied to the popular OpenAI Gym environments, of which a large variety can
readily be found on OpenAI’s website2 or in OpenAI’s corresponding GitHub repository3. Note that the
implementationcurrentlyonlysupportsthegenerationofasingleactionpertimestep. Inthefollowing, the
| design of | the implementation |     |     | will be | described. |     |     |     |     |     |
| --------- | ------------------ | --- | --- | ------- | ---------- | --- | --- | --- | --- | --- |
At the heart of the implementation lies the class ProximalPolicyOptimization, which is the class imple-
menting the actual PPO agent. This class allows for training and evaluating an agent’s policy featuring a
| policy network |     | as well | as a corresponding |     | state | value network. |     |     |     |     |
| -------------- | --- | ------- | ------------------ | --- | ----- | -------------- | --- | --- | --- | --- |
2https://gym.openai.com/
3https://github.com/openai/gym
25

Algorithm 1 Proximal Policy Optimization (PPO) using Stochastic Gradient Descent (SGD)
Input: N = Number of parallel agents collecting training data, T = Maximal trajectory length,
performance criterion or maximal number of training iterations, weighting factors v and h
π ←newPolicyNet()
θ
V ←newStateValueNetwork() (cid:46) Possibly parameter sharing with π
ω θ
env←newEnvironment()
| optimizer←newOptimizer(π |     |     |     | ,V ) |     |     |     |     |     |     |     |
| ------------------------ | --- | --- | --- | ---- | --- | --- | --- | --- | --- | --- | --- |
θ ω
|     |     |     | (cid:106) |     | (cid:107) |     |     |     |     |     |     |
| --- | --- | --- | --------- | --- | --------- | --- | --- | --- | --- | --- | --- |
number_minibatches = N∗T (cid:46) Compute number of minibatches per epoch
minibatch_size
while performance criterion not reached or maximal number of iterations not reached do
train_data←[]
| //  | Training | data  | collection | step. | Ideally | to be parallelized: |     |     |     |     |     |
| --- | -------- | ----- | ---------- | ----- | ------- | ------------------- | --- | --- | --- | --- | --- |
| for | actor =  | 1, 2, | ..., N     | do    |         |                     |     |     |     |     |     |
train_data←[]
s ←env.randomlyInitialize() (cid:46) Reset environment to a random initial state
t=1
|     | // Let                      | agent              | interact    | with its                        | environment  |           |           |           |          |                    |               |
| --- | --------------------------- | ------------------ | ----------- | ------------------------------- | ------------ | --------- | --------- | --------- | -------- | ------------------ | ------------- |
|     | // for                      | T time             | steps       | & collect                       | training     | data:     |           |           |          |                    |               |
|     | for t =                     | 1, 2,              | ..., T do   |                                 |              |           |           |           |          |                    |               |
|     | a ←π                        | .generate_action(s |             |                                 | )            |           |           |           |          |                    |               |
|     | t                           | θ                  |             |                                 | t            |           |           |           |          |                    |               |
|     | π θold                      | (a t |s t          | )←π θ       | .distribution.get_probability(a |              |           |           | t )       |          |                    |               |
|     | s                           | ,r ←env.step(a     |             | )                               |              |           |           |           | (cid:46) | Advance simulation | one time step |
|     | t+1                         | t                  |             | t                               |              |           |           |           |          |                    |               |
|     | train_data←train_data       |                    |             |                                 | + tuple(s    | t ,a t ,r | t ,π θold | (a t |s t | ))       |                    |               |
|     | // Use                      | training           | data        | to augment                      | each         | collected | tuple     |           |          |                    |               |
|     | // of training              |                    | data stored | in                              | train_data:  |           |           |           |          |                    |               |
|     | for t =                     | 1, 2,              | ..., T do   |                                 |              |           |           |           |          |                    |               |
|     | Vtarget                     |                    |             | +γ2r                            | +...+γT−t+1r |           |           | +γT−tV    |          |                    |               |
|     |                             | =r                 | t +γr       | t+1                             | t+2          |           | T−1       |           | ω        | (s T )             |               |
|     | t                           | =Vtarget−V         |             |                                 |              |           |           |           |          |                    |               |
|     | A                           |                    |             | (s )                            |              |           |           |           |          |                    |               |
|     | t                           | t                  |             | ω t                             |              |           |           |           |          |                    |               |
|     | train_data[t]←train_data[t] |                    |             |                                 | + tuple(A    |           | ,Vtarget) |           |          |                    |               |
t t
| optimizer.resetGradients(π |        |           |            | ,V  | )       |       |         |     |     |     |     |
| -------------------------- | ------ | --------- | ---------- | --- | ------- | ----- | ------- | --- | --- | --- | --- |
|                            |        |           |            | θ ω |         |       |         |     |     |     |     |
| //                         | Update | trainable | parameters |     | θ and ω | for K | epochs: |     |     |     |     |
| for                        | epoch  | = 1, 2,   | ..., K     | do  |         |       |         |     |     |     |     |
train_data←randomizeOrder(train_data)
|     | for mini_idx                                     |         | = 1, 2, | ..., number_minibatches |          |            | do  |     |     |           |     |
| --- | ------------------------------------------------ | ------- | ------- | ----------------------- | -------- | ---------- | --- | --- | --- | --------- | --- |
|     | M←getNextMinibatchWithoutReplacement(train_data, |         |         |                         |          |            |     |     |     | mini_idx) |     |
|     | for                                              | example | e ∈     | M do                    |          |            |     |     |     |           |     |
|     |                                                  | s ,a ,r | ,π      | (a |s ),A               | ,Vtarget | ←unpack(e) |     |     |     |           |     |
|     |                                                  | t t     | t θold  | t t                     | t t      |            |     |     |     |           |     |
_←π .generate_action(s ) (cid:46) Parameterize policy’s probabilit distribution
|     |     |         | θ         |                                 | t   |     |     |     |     |     |     |
| --- | --- | ------- | --------- | ------------------------------- | --- | --- | --- | --- | --- | --- | --- |
|     |     | π (a |s | )←π       | .distribution.get_probability(s |     |     |     | )   |     |     |     |
|     |     | θ t     | t         | θ                               |     |     |     | t   |     |     |     |
|     |     | p (θ)←  | πθ(at|st) |                                 |     |     |     |     |     |     |     |
t
πθold (at|st)
φ ←π .get_parameterization() (cid:46) To be computed in case of discrete action space
|     |       | t   | θstoch |          |            |         |                             |     |     |     |     |
| --- | ----- | --- | ------ | -------- | ---------- | ------- | --------------------------- | --- | --- | --- | --- |
|     | LCLIP | =   | 1      | (cid:80) | min(p (θ)A | ,clip(p | (θ),1−(cid:15),1+(cid:15))A |     |     | )   |     |
|     |       |     | |M|    |          | t          | t       | t                           |     |     | t   |     |
t∈{1,2,...,|M|}
|     |     | 1   | (cid:80) |     | )−Vtarget)2 |     |     |     |     |     |     |
| --- | --- | --- | -------- | --- | ----------- | --- | --- | --- | --- | --- | --- |
|     | LV  | =   |          | (V  | (s          |     |     |     |     |     |     |
|     |     | |M| |          | ω   | t           | t   |     |     |     |     |     |
t∈{1,2,...,|M|}
(cid:80)
H =− 1 φ log φ (cid:46) To be computed in case of discrete action space
|     |     | |M| |     | t   | t   |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
t∈{1,2,...,|M|}
|     | LCLIP+V+H                             |     | ←−LCLIP | +v∗LV |                | −h∗H |      |     |     |     |     |
| --- | ------------------------------------- | --- | ------- | ----- | -------------- | ---- | ---- | --- | --- | --- | --- |
|     | optimizer.backpropagate(π             |     |         |       | ,V ,LCLIP+V+H) |      |      |     |     |     |     |
|     |                                       |     |         |       | θ ω            |      |      |     |     |     |     |
|     | optimizer.updateTrainableParameters(π |     |         |       |                |      | ,V ) |     |     |     |     |
θ ω
return π
θ
Policy networks are implemented via the class Policy and feature three internal modules forming a pro-
26

cessing pipeline. The first part is the input module of the policy network, the second part is the output
module of the policy network, and the third part is an implementation of a Gaussian or Multinomial prob-
ability distribution provided by PyTorch. This processing pipeline of the three aforementioned parts works
as follows. The input module consumes a state representation and transforms it into some intermediate
representation. The resulting intermediate representation is then transformed into a parameterization for a
probability distribution using the output module. The probability distribution provided by PyTorch is then
parameterized using the parameterization produced by the output module. Actions are then drawn stochas-
ticallyfromtheprobabilitydistribution. Alternatively,theentropymaybecomputedforagivenprobability
distributionorthelog-probabilityofagivenactionmaybeobtainedafteraprobabilitydistributionhasbeen
parameterized. In the following, these three parts will be explained in more detail.
The input module may be of type, i.e. class, InCNN or InMLP, where the former implements a convo-
lutional neural network (CNN) architecture and the latter implements a feed-forward neural network (NN)
architecture. Both modules may be customized to some degree. Having two different options for instantiat-
ing an input module lets the user apply the same PPO agent, only using different input modules inside an
agent’sPolicy,todifferentobservationspaces. WhenusingaCNN,theagentmaybeappliedtoenvironments
involving visual state representations. If the observation space is non-visual, an input module consisting of
a feed-forward NN, as is the case when using an input module of class InMLP, may be more suitable. The
providedreferenceimplementationmayinferaninputmodule’srequirednetworkarchitectureautomatically
from a provided Gym environment instance.
Whiletherearemultipleclassesofinputmodules,thereisonlyoneclassofoutputmodules. Thisclassis
called OutMLP and implements a feed-forward NN. The number of output nodes inside an output module
is automatically chosen in accordance with the requirements imposed by an environment.
Generally, the implementation currently only supports generating a single action a per state s . When
t t
facing an environment featuring a continuous action space, the output module computes a single mean µ
t
to parameterize a one-dimensional Gaussian probability distributions (called Normal in the reference imple-
mentation). The Gaussian’s standard deviation is a hyperparameter to be chosen by the experimenter. It
may either be set to a fixed value (as is usually the case in PPO) or (linearly or exponentially) annealed
betweentwovaluesortrainedusingSGD.Ifthestandarddeviationistrainable,thepolicynetworkdoesnot
onlycomputeameanµ toparameterizeagivenGaussiandistribution,butalsoasecondvaluetakentobea
t
so-calledlog standard deviation, log(σ ). Thelogstandarddeviationisthentransformedintoaregularstan-
t
dard deviation σ by applying the exponential function to the log standard deviation, i.e. σ =exp(log(σ )).
t t t
This is done to enforce non-negative standard deviations. The resulting standard deviation is then used
to parameterize a given Gaussian distribution. How the procedure for training standard deviation works
in detail, including back-propagation-paths, is explained in [16]. For Gym environments featuring discrete
action spaces, a Multinomial probability distribution (called Categorical in the reference implementation) is
parameterized given the outputs generated by the output module. In this case, an output module has as
many output nodes as there are categories, i.e. elements, in the corresponding action space A.
Since training a PPO agent involves both training a policy network and a state-value network, also a
state-value network class has been implemented, which is called ValueNet in the reference implementation.
A state-value network consists of both an input- and an output module, where the input module may be
shared with the policy network, and the output module predicts a state’s value given the intermediate rep-
resentation produced by the input module.
To increase the training efficiency, the provided reference implementation makes use of vectorized Gym
environments. Vectorization here refers to stacking multiple parallel Gym environments of the same kind
together and letting multiple PPO agents interact with these environments in parallel. To be more precise,
each agent interacts with its private instance of a Gym environment. This speeds up the training data
generation step, since multiple agents can generate training data for the next update step in parallel rather
than sequentially.
27

Note that some Gym environments, e.g. the implementations of the Atari 2600 environments4, do not
provide Markovian state representations by default. For those cases, functionality has been implemented to
stackmultipleconsecutivenon-MarkovianstaterepresentationstogetherinordertoartificiallybuildMarko-
vianstate-representationsfromthenon-MarkovianonesdirectlyprovidedbytheseGymenvironments. Note
that state representations are called observations in the reference implementation.
Moreover, PyTorch’s auto-differentiation capability is used to perform back-propagation.
Also, the ProximalPolicyOptimization class provides basic evaluation capabilities. These are two-fold.
Forausertogainasubjectiveimpressionoftheperformanceofatrainedagent,theProximalPolicyOptimiza-
tion class allows for letting a trained agent act in its environment while visually displaying, i.e. rendering,
the environment. Alternatively, for a more objective analysis, basic quantitative analysis of the learning
outcome of a PPO agent is supported in that the implementation allows for collecting and saving basic
statistics concerning the training progress and corresponding evaluations. This will be demonstrated in the
following subsection.
Furthermore,forconvenience,configuration("config")filesmaybeusedtospecifyinwhichconfiguration
to train a PPO agent. Numerous hyperparameter settings may be controlled by adapting some example
configuration files or adding new ones. Moreover, paths may be specified for saving or loading an agent’s
trained network architectures. This may be useful for saving trained agents and visually inspecting their
performance at a later time point. Note also that both policy- and state-value networks may be saved to
possibly continue training later.
Inline-comments have been added to the reference implementation to facilitate better understanding of
the produced code. Again, the reference implementation can be found at https://github.com/Bick95/PPO.
4.2 Evaluation of Reference Implementation
In the following, the provided reference implementation will be evaluated. For this, PPO agents have been
trained on two different OpenAI Gym environments. Below, the quantitative analysis of the training out-
comes will be shown. Also a short discussion of the observed results will be provided.
In a first task, PPO agents have been trained on the OpenAI Gym MountainCarContinuous-v0 environ-
ment featuring a continuous action space as well as a continuous observation space. In this environment, an
agentsteersacar,placedbetweentwosurroundingmountains,todriveforthandback. Theagent’sgoalisto
control the car in such a way that the car gains enough momentum to be able to reach the flag at the top of
one mountain. In this environment, agents have been trained for 3 million state transitions. In one training
condition, the standard deviation of the Gaussian, from which actions are sampled, has been fixed. In a
second training condition, the standard deviation has been a trainable parameter, being trained using SGD
(as explained in Section 4.1). The configuration files, specifying exactly how the agents have been trained
andtestedonthegiventask,usingeitherafixedortrainablestandarddeviation,canbefoundonGitHub. In
bothtrainingconditions,i.e. fixedversustrainablestandarddeviation,bothastochasticandadeterministic
evaluation have been performed. During a stochastic evaluation, actions a are stochastically sampled from
t
a Gaussian distribution, while actions are selected using a standard deviation of 0 during a deterministic
evaluation. Furthermore, the results reported below have been obtained by training ten independent agents
per training condition and averaging the ten independent results per testing condition.
For each testing condition, two metrics have been measured after the end of the training procedure. One
metric is the total reward accumulated over 10,000 time steps and the second reward is the total number of
restarts of an agent’s environment during the 10,000 aforementioned time steps performed during the final
evaluation. Here, a higher number of restarts indicates that an agent has achieved its goal more frequently.
The accumulated reward increases every time that the car controlled by the agent reaches its goal, while
it is decreased as a function of the energy consumed by the car. Tables 2 and 3 show the total number of
restarts and the total accumulated rewards (both averaged over ten independent test runs), respectively.
4https://gym.openai.com/envs/atari
28

Evaluation
|     |     |           |           | Stochastic   | Deterministic |
| --- | --- | --------- | --------- | ------------ | ------------- |
|     |     | Standard  | Fixed     | 52.5 (21.28) | 46.1 (25.79)  |
|     |     | Deviation | Trainable | 43.3 (21.69) | 36.6 (20.53)  |
Table 2: Total number of restarts of an agent’s environment within 10,000 time steps performed during the
final evaluation. The results have been averaged over 10 randomly initialized test runs. Measurements have
beentakenintheOpenAIGymMountainCarContinuous-v0environmentfeaturingbothacontinuousaction
and state space. The corresponding training and testing configuration files for fixed and trainable standard
| deviations | can be found | here | and here, respectively. |     |     |
| ---------- | ------------ | ---- | ----------------------- | --- | --- |
First,considerthetotalnumberofrestartsunderthefourdifferenttestingconditionspresentedinTable2.
Whenonlyconsideringstochasticevaluations,nostatisticallysignificantdifferencecanbefoundbetweenthe
two training conditions, i.e. when comparing the results obtained using a fixed standard deviation to those
obtained using a trainable standard deviation. Likewise, when only considering deterministic evaluations,
also no statistically significant difference can be found between the two training conditions.
When fixing the standard deviation and comparing the total number of restarts between the stochastic
and deterministic evaluation, also no statistically significance is found. Moreover, no statistically significant
differenceisfoundwhentrainingthestandarddeviationandcomparingthetotalnumberofrestartsbetween
| the stochastic | and deterministic |     | evaluation. |     |     |
| -------------- | ----------------- | --- | ----------- | --- | --- |
When performing the same set of comparisons, as explained above for the total number of restarts, to
the total accumulated rewards, as shown in Table 3 below, the same results, i.e. no statistically significant
| differences, | are found. |     |     |     |     |
| ------------ | ---------- | --- | --- | --- | --- |
Inconclusion,thismeansthefollowing. Whilethereappeartoberelevantdifferencesinthequalityofthe
learning outcome depending on whether the standard deviation is fixed or trained, those differences are not
statisticallysignificantatthe0.05level. Also,thereappeartoberelevantdifferencesinthelearningoutcome
depending on whether the final evaluation is performed stochastically or deterministically. However, also
| those differences | are | not statistically | significant | at the 0.05 level. |     |
| ----------------- | --- | ----------------- | ----------- | ------------------ | --- |
While the results provided above suggest that there is no measurable advantage of training the standard
deviationasopposedtokeepingitconstant,asiscommonlydoneinPPO,theremaystillbereasonstotrain
the standard deviation. Note that the fixed standard deviation value, used in the experiments presented
above, hasbeenobtainedbyinspectingtowhichvaluethestandarddeviationconvergedduringonetraining
runwhiletreatingthestandarddeviationasatrainableparameter. Priortrialanderrorsearchofappropriate
settings of the fixed standard deviation parameter had not been successful. The manually tested values led
toover-explorationorunder-exploration. Thus, incaseswherechoosingastandarddeviationparameterisa
challenging task, treating the standard deviation as a trainable parameter might reveal appropriate choices
| for a fixed | standard | deviation | value. |     |     |
| ----------- | -------- | --------- | ------ | --- | --- |
Evaluation
|     |           |     |           | Stochastic        | Deterministic        |
| --- | --------- | --- | --------- | ----------------- | -------------------- |
|     | Standard  |     | Fixed     | 3609.84 (2360.32) | 3899.43 (2783.13)    |
|     | Deviation |     | Trainable | -4.61 (1.46)      | -23703.79 (83220.94) |
Table 3: Total number of accumulated rewards received from an agent’s environment within 10,000 time
steps performed during the final evaluation. The results have been averaged over 10 randomly initialized
test runs. Measurements have been taken in the OpenAI Gym MountainCarContinuous-v0 environment
featuring both a continuous action and state space. The corresponding training and testing configuration
files for fixed and trainable standard deviations can be found here and here, respectively.
Inasecondtask, PPOagentshavebeentrainedontheOpenAIGymCartPole-v0environmentfeaturing
a discrete action space and a continuous observation space. The goal of an agent in this environment is to
29

command a cart to move horizontally to the left or right, such that a pole, placed vertically on top of the
cart, remains balanced without falling over to the left or right. In this environment, rewards are emitted for
every time step that the pole remains balanced without falling over. Since environments get immediately
restarted as soon as an agent fails on the given task, only the total number of restarts serves as a sensible
metric to assess an agent’s learning outcome in this environment. Here, a lower number of restarts indicates
thattheagenthassuccessfullymanagedbalancingthepoleforlongerperiodsoftimebeforeanenvironment
had to be restarted. Ten randomly initialized agents have been trained on this task for 200,000 time steps
each. The aforementioned metric, i.e. the total number of restarts, has been measured during a stochastic
andadeterministicevaluation(for10,000timestepseach)pertrainedagent. Duringastochasticevaluation,
actions have been sampled stochastically, while always the action associated with the highest probability
massinagivenstatehasbeenchoseninthedeterministicevaluationcondition. Themeasurementsreported
below have been computed as the average over the ten independent test runs per testing condition. The
whole training and testing configuration can be found in the corresponding configuration file on GitHub.
The results are as follows.
Duringthestochasticevaluation, 78.0restartshavebeenobservedonaverage(withastandarddeviation
of18.35). Duringthedeterministicevaluation,53.7restartshavebeenobservedonaverage(withastandard
deviation of 5.08). These findings are significantly different (p-value: 0.00078 < 0.05).
Thismeansthat,inatleastoneoftwotasksconsideredinthisreport,usingadeterministicpolicyduring
the evaluation of an agent has led to significantly better evaluation results than performing the evaluation
on the same policy run stochastically.
The aforementioned observation is important for the following reason. Contemporary research in the
field of DRL sometimes mainly focuses on comparing the learning speed of agents [4] or the average scores
obtained during training of an agent [6]. These are metrics being computed based on the performance of
policiesrunstochastically. However, inreallifeapplications, theremaybesituationswhererunningapolicy
stochastically may result in catastrophic errors, such that in some occasions policies might have to be run
deterministically after the end of the training phase. For example, consider the physical damage that may
arisefromarobotperformingsurgeryonapatientbasedonastochasticpolicy. Theresultspresentedabove
seem to suggest that assessing the quality of the learning outcome of a DRL algorithm purely based on the
results obtained during the evaluation of a stochastic policy might not accurately reflect the results that
would be obtained when running a resulting policy deterministically. Given the above considerations, it
mightbeavaluablecontributiontothefieldofDRLinthefutureifresearcherscometofocusmorestrongly
onthedifferencesbetweenrunningpoliciesinastochasticandadeterministicmodeaftertheendoftraining.
5 Considerations and Discussion of PPO
Throughoutthisreport,thePPOalgorithmhasbeenpresentedandexplainedinalotofdetail. Bynow,the
reader is assumed to know in detail how the algorithm works. Furthermore, the Introduction (see Section 1)
and later Sections listed some reasons for using PPO, thereby justifying the importance of giving a detailed,
thorough explanation of the algorithm for the first time in this report. Some of these reasons for using PPO
were its comparatively high data efficiency, its ability to cope with various kinds of action spaces, and its
robust learning performance [6]. However, since this report aims at providing a neutral view on PPO and
thefieldofDRLingeneral,thissectionwilladdresssomecriticalconsiderationsconcerningPPOandrelated
methods. Also, the field of DRL will be considered from a broader perspective.
Particularly, thefirstsubsection, Section5.1, willconsiderreasonsforwhyPPOmightnotalwaysbethe
most suitable DRL algorithm. Critical aspects associated with PPO will be discussed.
The second subsection, Section 5.2, will be concerned with some macro-level considerations, addressing
the question whether using PPO or comparable methods might lead to the emergence of General Artificial
Intelligence (GAI) at some point in the future.
30

5.1 Critical Considerations concerning PPO
The following will address some of the limitations of the PPO algorithm.
In the original paper, the inventors of PPO argue that PPO’s main objective function, LCLIP, is de-
signedtopreventexcessivelylargeweight(i.e. trainableparameter)updatesfromhappening. Thisisbecause
LCLIP is supposed to remove the incentive for moving the probability ratio p (θ) outside a certain interval
t
within a single weight update step consisting of multiple epochs of weight updates. That this procedure
will not entirely prevent destructively large weight updates from happening is obvious already from the fact
that there is no hard constraint enforcing this condition. The authors of [15] analyzed the effectiveness of
LCLIP in preventing weight updates, which would effectively move the probability ratio outside the interval
[1−(cid:15),1+(cid:15)], fromhappening. TheyfoundthatLCLIP hadatleastsomeeffectinrestrictingtheevolutionof
p (θ), but generally failed to contain p (θ) strictly inside the interval [1−(cid:15),1+(cid:15)]. As a possible solution to
t t
this problem, the authors of [15] proposed a variant of PPO, which they call Trust Region-based PPO with
Rollback (TR-PPO-RB). According to [15], TR-PPO-RB exhibits better learning performance and higher
sample efficiency across many learning tasks compared to vanilla PPO.
Speaking about data efficiency, it must be mentioned that PPO’s sample efficiency is comparatively low.
Ontheonehand, PPO’ssampleefficiencyisindeedhigherthanthatofmanyotherpolicygradientmethods
(PGMs). This is because PPO allows for multiple epochs of weight updates using the same freshly sam-
pled training data, whereas many other PGMs may perform only a single epoch of weight updates on the
obtained training data [6]. On the other hand, PGMs, serving as a means of reference here and commonly
being on-policy methods [25], are generally associated with lower data efficiency than off-policy methods
[25], which makes the whole aforementioned comparison between PPO and other PGMs look less spectac-
ular. This makes PPO a less-optimal choice when facing learning tasks, where training data is expensive
ordifficulttoobtain. Insuchsituations,moresampleefficientDRLalgorithmsmightbemoresuitabletouse.
Also, it must be mentioned that PPO, being an on-policy method, is only applicable to learning tasks
being on-policy compatible.
Another no-trivial aspect about PPO is hyperparameter tuning. In many cases, PPO performs report-
edly well without performing much parameter tuning [15]. However, in cases where parameter tuning is
still required, this task is non-trivial, since there is no intuitive way of determining whether, for example,
largerorsmallervaluesforthehyperparameter(cid:15)wouldimproveaPPOagent’slearningperformanceaswell
as possibly its sample efficiency. Likewise, there is no way of determining a suitable number of epochs (of
weight updates) per weight update step in advance. Those values possibly have to be fine-tuned using the
expensive method of parameter sweeping when the training outcome is worse than expected or desired.
A related issue concerns the setting of the standard deviation hyperparameter σ when sampling actions
from continuous action spaces. As reported above (see Section 3.2.1), the inventors of PPO proposed to set
the standard deviation to a fixed value. The question arises what justifies setting the standard deviation
to some fixed value. This question arises because there is no reason provided for this particular choice of
determiningthestandarddeviationhyperparameter. Aproblemrelatedtohavingafixedstandarddeviation
is that a standard deviation of a certain value might be a very small or very large value, depending on the
action space at hand. If the standard deviation is fixed to a comparatively small value, this might hinder
exploration of the state-action space. If the standard deviation is fixed to a comparatively large value, this
might lead to over-exploration of the state-action space, thus slowing down convergence of the policy. A
possible alternative to the proposed way of fixing a Gaussian’s standard deviation treats the standard de-
viation as a trainable parameter [16], as explained in Section 4.1. While this way of training the standard
deviationhasbeenmotivatedintheliterature[16], theevaluationofthereferenceimplementation, provided
inSection4.2,failedtodemonstrateameasurableadvantageoftrainingthestandarddeviationasopposedto
keepingitconstant,asiscommonlydoneinPPO.However,asreportedinSection4.2,trainingthestandard
deviation might still reveal appropriate choices for a fixed standard deviation value.
31

Another aspect to be mentioned about PPO concerns the circumstance that PPO is a model-free DRL
algorithm. Recallthatmodel-free referstoaDRLagentnotlearninganexplicitmodeloftheenvironmentitis
situatedin,whilemodel-basedDRLalgorithmslearnsomeformofexplicitrepresentationoftheenvironment
surrounding them [5]. As argued in [5], model-based DRL algorithms stand out due to their enhanced
capability of transferring generalized knowledge about their environment (encoded in their world model)
between tasks. Also, model-based DRL algorithms may perform planning by simulating potential future
outcomesoftheirpresentandfuturedecisionmakingandmayevenlearntosomeextentfromofflinedatasets
[5]. PPO, on the contrary, being a model-free DRL algorithm, naturally lacks these features.
5.2 RL in the Context of Artificial Intelligence (AI)
In the following, the question will be touched upon whether RL may be a suitable means of developing
General Artificial Intelligence (GAI) in the future.
In a recent paper, Silver et al. [34] argue that all there is needed in order to form intelligent behavior in
an agent is a reward metric which is to be maximized through some learning procedure implemented by an
agent. More specifically, their hypothesis, called Reward-is-Enough, states that [34]:
Hypothesis 1 (Reward-is-Enough): Intelligence, and its associated abilities, can be understood as sub-
serving the maximisation of reward by an agent acting in its environment.
The idea behind the Reward-is-Enough hypothesis is as follows. If an agent is presented to some envi-
ronment in which it has to learn to act in a way such that a given cumulative reward metric gets maximize,
the agent will ultimately discover increasingly complex behaviors in order to achieve the goal of maximizing
the given reward metric. Thereby, on the long run, an agent will develop enough sophisticated abilities to
be eventually considered intelligent. This hypothesis is relevant to the topic considered here, since Silver et
al. [34] argue that the reward maximization task discussed in their paper is perfectly compatible with the
concept of RL. Thus, according [34], RL may possibly give rise to the emergence of GAI in the future.
Silveretal. [34]alsoarguethattheirhypothesismightevengiveanappropriateaccountoftheemergence
of natural intelligence present in animals, including the human kind. The approach of trying to explain the
emergence of natural intelligence as a by-product of solving a singular problem, namely that of evolutionary
pressure,hasalreadybeenadoptedinearlyworkonArtificialIntelligence[35]andinthefieldofEvolutionary
Psychology [36]. Thus, there seems to be a lot of support for believing in this hypothesis as a likely solution
to the questions of how and why intelligence has ultimately arisen in nature.
However, when using the Reward-is-Enough hypothesis to explain both the emergence of natural intel-
ligence and how to arrive at GAI (e.g. through RL) in the future, one must beware of a subtle difference
between those two cases. When talking about the evolution of natural intelligence, one considers the evolu-
tionaryprocesshappeningtoanentirepopulationofindividuals, butnottheevolutionofintelligencewithin
an individual. When considering the future emergence of GAI in the context of the Reward-is-Enough hy-
pothesis, however, one concerns oneself with the question of how GAI may emerge within a single artificial
agent having a possibly infinite life span. This difference is due to the following reason. There is some
evidence suggesting that human intelligence is dependent on biological factors [37, 38]. Thus, if the level of
general intelligence is prescribed to a biological being, e.g. by a so-called factor g [37], natural intelligence
cannot be caused by reward maximization within an individual at the same time. Note that this subtle dif-
ference, whichisalsoacknowledgedbySilveretal. [34], isofimportanceinthatitindicatesthatthesuccess
ofnaturalevolutioninformingnaturalintelligencecannotbeseenasdirectsupportfortheplausibilityofthe
Reward-is-Enough hypothesis in the context of developing GAI. Instead, the truth of this broad hypothesis
will ultimately have to be demonstrated by providing a proof of concept, i.e. an example demonstrating the
practical working of this hypothesis.
Especially since RL is said to be a suitable means of testing the Reward-is-Enough hypothesis [34], pro-
viding a practical implementation demonstrating the truth of this hypothesis is of particular importance,
since no case is publicly known yet in which GAI has emerged from training RL agents in spite of the large
body of corporate and academic research that has been conducted on the field of RL already.
32

In the following, I would like to point out a few more theoretical considerations challenging the idea that
GAI may arise from RL.
Firstofall,recallthatcontemporaryresearchonRLdrawsupon(recurrent)neuralnetworkarchitectures
to implement agents’ decision making strategies. So far, it is an open question whether the model capacity
of network architectures that can possibly be trained on today’s available hardware is sufficiently large to
accommodate decision making strategies causing truly intelligent behavior. Also, ultimately all vanilla (re-
current) neural network architectures can be seen as singular, sequential streams of information processing.
On the contrary, biological brains (being the only known source of true intelligence yet) are composed of up
to billions of neurons [38], which have often been observed to form clusters being associated with dedicated
cognitiveabilities[38],workinginparallel. Fromthispointofview,onemayaskwhetherutilizingsequential
streamsofinformationprocessingonly,notdrawingupondistributedsystemarchitecturesallowingfortruly
parallel information processing as is happening in biological brains as well, is an appropriate approach to
seek the emergence of GAI. Also, as argued in the literature [35], evolutionary processes often fail to deliver
the most efficient solutions to certain problems. Thus, approaching the problem of developing GAI from
an engineering perspective, rather than from an evolutionary perspective, might potentially lead to more
efficient solutions eventually.
6 Conclusion
This report began by giving a short introduction into the field of Reinforcement Learning (RL) and Deep
Reinforcement Learning (DRL), particularly focusing on policy gradient methods (PGMs) and the class of
REINFORCE algorithms. Then, Proximal Policy Optimization (PPO), largely following the principles of
REINFORCE, has been introduced, pointing out the poor documentation of this algorithm. Acknowledging
the importance of PPO, however, this report continued explaining the PPO algorithm in minute detail.
Afterwards, an easy to comprehend reference implementation of PPO has been introduced and assessed.
Finally,somecriticalremarkshavebeenmademadeaboutthedesignofPPOanditsrestrictedapplicability.
Also, the question whether RL may lead to the emergence of General Artificial Intelligence in the future
has been addressed. Given the undisputed importance of RL, this report concludes by once again pointing
out the importance of delivering adequate documentation of RL algorithms to be introduced in the future.
Acknowledging the amount of future work that is still to be done in advancing the field of RL, researchers
workinginthisfieldtothepresentdateoughtpresenttheirfindingsandproposedmethodsinawaythatcan
be well understood not only by other experts with life-long experience in their field, but also by tomorrow’s
scientists just about to dive into the broad and exciting field of RL.
References
[1] V.Mnih,K.Kavukcuoglu,D.Silver,A.A.Rusu,J.Veness,M.G.Bellemare,A.Graves,M.Riedmiller,
A.K.Fidjeland,G.Ostrovski,etal.,“Human-levelcontrolthroughdeepreinforcementlearning,” Nature,
vol. 518, no. 7540, pp. 529–533, 2015.
[2] V.Mnih,K.Kavukcuoglu,D.Silver,A.Graves,I.Antonoglou,D.Wierstra,andM.Riedmiller,“Playing
atari with deep reinforcement learning,” arXiv preprint arXiv:1312.5602, 2013.
[3] R. S. Sutton and A. G. Barto, Reinforcement Learning: An Introduction. MIT press, 2018.
[4] V. Mnih, A. P. Badia, M. Mirza, A. Graves, T. Lillicrap, T. Harley, D. Silver, and K. Kavukcuoglu,
“Asynchronous methods for deep reinforcement learning,” in International Conference on Machine
Learning, pp. 1928–1937, 2016.
[5] D. Hafner, T. Lillicrap, M. Norouzi, and J. Ba, “Mastering atari with discrete world models,” arXiv
preprint arXiv:2010.02193, 2020.
[6] J. Schulman, F. Wolski, P. Dhariwal, A. Radford, and O. Klimov, “Proximal policy optimization algo-
rithms,” arXiv preprint arXiv:1707.06347, 2017.
33

[7] L. Rejeb, Z. Guessoum, and R. M’Hallah, “The exploration-exploitation dilemma for adaptive agents,”
in Proceedings of the Fifth European Workshop on Adaptive Agents and Multi-Agent Systems, Citeseer,
2005.
[8] M. Fortunato, M. G. Azar, B. Piot, J. Menick, I. Osband, A. Graves, V. Mnih, R. Munos, D. Hassabis,
O. Pietquin, et al., “Noisy networks for exploration,” arXiv preprint arXiv:1706.10295, 2017.
[9] M. G. Bellemare, Y. Naddaf, J. Veness, and M. Bowling, “The arcade learning environment: An eval-
uation platform for general agents,” Journal of Artificial Intelligence Research, vol. 47, pp. 253–279,
2013.
[10] A. Krizhevsky, I. Sutskever, and G. E. Hinton, “Imagenet classification with deep convolutional neural
networks,” Advances in Neural Information Processing Systems, vol. 25, pp. 1097–1105, 2012.
[11] D. Silver, A. Huang, C. J. Maddison, A. Guez, L. Sifre, G. Van Den Driessche, J. Schrittwieser,
I. Antonoglou, V. Panneershelvam, M. Lanctot, et al., “Mastering the game of go with deep neural
| networks | and | tree | search,” | Nature, | vol. | 529, no. 7587, | pp. 484–489, | 2016. |
| -------- | --- | ---- | -------- | ------- | ---- | -------------- | ------------ | ----- |
[12] D. Silver, J. Schrittwieser, K. Simonyan, I. Antonoglou, A. Huang, A. Guez, T. Hubert, L. Baker,
M. Lai, A. Bolton, et al., “Mastering the game of go without human knowledge,” Nature, vol. 550,
| no. 7676, | pp. | 354–359, |     | 2017. |     |     |     |     |
| --------- | --- | -------- | --- | ----- | --- | --- | --- | --- |
[13] D.Silver,T.Hubert,J.Schrittwieser,I.Antonoglou,M.Lai,A.Guez,M.Lanctot,L.Sifre,D.Kumaran,
T.Graepel,etal.,“Ageneralreinforcementlearningalgorithmthatmasterschess,shogi,andgothrough
| self-play,” |     | Science, | vol. | 362, no. | 6419, pp. | 1140–1144, | 2018. |     |
| ----------- | --- | -------- | ---- | -------- | --------- | ---------- | ----- | --- |
[14] C.Berner,G.Brockman,B.Chan,V.Cheung,P.Debiak,C.Dennison,D.Farhi,Q.Fischer,S.Hashme,
C.Hesse,R.Józefowicz,S.Gray,C.Olsson,J.Pachocki,M.Petrov,H.P.deOliveiraPinto,J.Raiman,
T. Salimans, J. Schlatter, J. Schneider, S. Sidor, I. Sutskever, J. Tang, F. Wolski, and S. Zhang, “Dota
2 with large scale deep reinforcement learning,” arXiv preprint arXiv:1912.06680, 2019.
[15] Y. Wang, H. He, and X. Tan, “Truly proximal policy optimization,” in Uncertainty in Artificial Intelli-
| gence, | pp. | 113–122, | PMLR, | 2020. |     |     |     |     |
| ------ | --- | -------- | ----- | ----- | --- | --- | --- | --- |
[16] R. J. Williams, “Simple statistical gradient-following algorithms for connectionist reinforcement learn-
| ing,” | Machine | Learning, |     | vol. 8, no. | 3-4, | pp. 229–256, | 1992. |     |
| ----- | ------- | --------- | --- | ----------- | ---- | ------------ | ----- | --- |
[17] Y. Duan, X. Chen, R. Houthooft, J. Schulman, and P. Abbeel, “Benchmarking deep reinforcement
learning for continuous control,” in International Conference on Machine Learning, pp. 1329–1338,
| PMLR,      | 2016. |          |          |      |         |          |             |       |
| ---------- | ----- | -------- | -------- | ---- | ------- | -------- | ----------- | ----- |
| [18] C. J. | C. H. | Watkins, | Learning | from | Delayed | Rewards. | PhD thesis, | 1989. |
[19] C. J. Watkins and P. Dayan, “Q-learning,” Machine Learning, vol. 8, no. 3-4, pp. 279–292, 1992.
[20] J. Peters and J. A. Bagnell, Policy Gradient Methods, pp. 1–4. Boston, MA: Springer US, 2016.
[21] V.R.KondaandJ.N.Tsitsiklis,“Actor-criticalgorithms,” inAdvancesinNeuralInformationProcessing
| Systems, | pp. | 1008–1014, |     | Citeseer, | 2000. |     |     |     |
| -------- | --- | ---------- | --- | --------- | ----- | --- | --- | --- |
[22] I. Goodfellow, Y. Bengio, and A. Courville, Deep Learning. MIT Press, 2016. http://www.
deeplearningbook.org.
[23] J.Ba,V.Mnih,andK.Kavukcuoglu,“Multipleobjectrecognitionwithvisualattention,” arXivpreprint
| arXiv:1412.7755, |     |     | 2014. |     |     |     |     |     |
| ---------------- | --- | --- | ----- | --- | --- | --- | --- | --- |
[24] S. Gu, T. Lillicrap, Z. Ghahramani, R. E. Turner, B. Schölkopf, and S. Levine, “Interpolated policy
gradient: Merging on-policy and off-policy gradient estimation for deep reinforcement learning,” arXiv
| preprint | arXiv:1706.00387, |     |     | 2017. |     |     |     |     |
| -------- | ----------------- | --- | --- | ----- | --- | --- | --- | --- |
34

[25] J. P. Hanna and P. Stone, “Towards a data efficient off-policy policy gradient.,” in AAAI Spring Sym-
| posia, 2018. |     |     |     |     |
| ------------ | --- | --- | --- | --- |
[26] S.Paternain,J.A.Bazerque,A.Small,andA.Ribeiro,“Stochasticpolicygradientascentinreproducing
kernel hilbert spaces,” IEEE Transactions on Automatic Control, vol. 66, no. 8, pp. 3429–3444, 2021.
[27] J. Schulman, S. Levine, P. Abbeel, M. Jordan, and P. Moritz, “Trust region policy optimization,” in
International Conference on Machine Learning, pp. 1889–1897, PMLR, 2015.
[28] J.S.Bridle,“Trainingstochasticmodelrecognitionalgorithmsasnetworkscanleadtomaximummutual
informationestimationofparameters,” inAdvances in Neural Information Processing Systems,pp.211–
| 217, 1990. |     |     |     |     |
| ---------- | --- | --- | --- | --- |
[29] J.PengandR.J.Williams,“Incrementalmulti-stepq-learning,” inMachine Learning Proceedings 1994,
| pp. 226–232, | Elsevier, 1994. |     |     |     |
| ------------ | --------------- | --- | --- | --- |
[30] J. Schulman, P. Moritz, S. Levine, M. Jordan, and P. Abbeel, “High-dimensional continuous control
using generalized advantage estimation,” arXiv preprint arXiv:1506.02438, 2015.
[31] S. Kakade and J. Langford, “Approximately optimal approximate reinforcement learning,” in In Proc.
| 19th International | Conference | on Machine | Learning, | Citeseer, 2002. |
| ------------------ | ---------- | ---------- | --------- | --------------- |
[32] Z.Liu, B.Wu, W.Luo, X.Yang, W.Liu, andK.-T.Cheng, “Bi-realnet: Enhancingtheperformanceof
1-bit cnns with improved representational capability and advanced training algorithm,” in Proceedings
of the European Conference on Computer Vision (ECCV), pp. 722–737, 2018.
[33] B. Baker, O. Gupta, N. Naik, and R. Raskar, “Designing neural network architectures using reinforce-
| ment learning,” | arXiv preprint | arXiv:1611.02167, |     | 2016. |
| --------------- | -------------- | ----------------- | --- | ----- |
[34] D. Silver, S. Singh, D. Precup, and R. S. Sutton, “Reward is enough,” Artificial Intelligence, vol. 299,
| p. 103535, | 2021. |     |     |     |
| ---------- | ----- | --- | --- | --- |
[35] R. Davis, “What are intelligence? and why? 1996 aaai presidential address,” AI Magazine, vol. 19,
| no. 1, pp.       | 91–91, 1998.  |               |             |                  |
| ---------------- | ------------- | ------------- | ----------- | ---------------- |
| [36] L. Cosmides | and J. Tooby, | “Evolutionary | psychology: | A primer,” 1997. |
[37] T. J. Bouchard, “Genes, evolution and intelligence,” Behavior Genetics, vol. 44, no. 6, pp. 549–577,
2014.
[38] J. W. Kalat, Biological Psychology. Boston, MA: Cengage Learning, 2019.
35

## Extracted Images

### Page 1

![page001_img001.png](img/page001_img001.png)
