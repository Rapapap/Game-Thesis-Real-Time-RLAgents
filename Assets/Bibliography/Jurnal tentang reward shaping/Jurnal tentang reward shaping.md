|     |     | Reward | Models |              | in  | Deep            | Reinforcement |              | Learning:     |     |     | A Survey |     |     |     |
| --- | --- | ------ | ------ | ------------ | --- | --------------- | ------------- | ------------ | ------------- | --- | --- | -------- | --- | --- | --- |
|     |     |        | RuiYu, | ShenghuaWan, |     |                 | YucenWang,    |              | Chen-XiaoGao, |     |     |          |     |     |     |
|     |     |        |        | LeGan,       |     | ZongzhangZhang, |               | De-ChuanZhan |               |     |     |          |     |     |     |
NationalKeyLaboratoryforNovelSoftwareTechnology,NanjingUniversity,China
SchoolofArtificialIntelligence,NanjingUniversity,China
{yur,wansh,wangyc,gaocx}@lamda.nju.edu.cn,{ganle,zzzhang,zhandc}@nju.edu.cn
Abstract
|     |     |     |     |     |     |     |     | byenablingtheagenttoexplore, |     |     |     | adapt, | andoptimizeitsbe- |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | ---------------------------- | --- | --- | --- | ------ | ----------------- | --- | --- |
5202 nuJ 81  ]GL.sc[  1v12451.6052:viXra
haviorbasedontheoutcomeofitsactions,therebyachieving
Inreinforcementlearning(RL),agentscontinually unprecedentedlevelsofautonomyandcapability.
interactwiththeenvironmentandusethefeedback Akeycomponentofreinforcementlearningisthereward,
| torefinetheirbehavior. |         |             | Toguidepolicyoptimiza- |      |      |            |     |                   |         |             |      |                |          |           |          |
| ---------------------- | ------- | ----------- | ---------------------- | ---- | ---- | ---------- | --- | ----------------- | ------- | ----------- | ---- | -------------- | -------- | --------- | -------- |
|                        |         |             |                        |      |      |            |     | which essentially |         | defines     | the  | goal of        | interest | in the    | task and |
| tion,                  | reward  | models      | are introduced         |      | as   | proxies of |     |                   |         |             |      |                |          |           |          |
|                        |         |             |                        |      |      |            |     | guides the        | agents  | to optimize |      | their behavior |          | toward    | that in- |
| the                    | desired | objectives, | such                   | that | when | the agent  |     |                   |         |             |      |                |          |           |          |
|                        |         |             |                        |      |      |            |     | tent [Sutton      | et al., | 1998].      | Just | as dopamine    |          | motivates | and      |
maximizes the accumulated reward, it also fulfills reinforcesadaptiveactionsinbiologicalsystems, rewardsin
the task designer’s intentions. Recently, signifi- RL encourage exploration of the environment and guide in-
| cant | attention | from | both | academic | and | industrial |     |     |     |     |     |     |     |     |     |
| ---- | --------- | ---- | ---- | -------- | --- | ---------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
telligentagentstowardsdesiredbehaviors[Glimcher,2011].
researchershasfocusedondevelopingrewardmod-
|     |          |      |               |      |     |             |     | However,     | while   | rewards | are typically |        | predefined | in    | research  |
| --- | -------- | ---- | ------------- | ---- | --- | ----------- | --- | ------------ | ------- | ------- | ------------- | ------ | ---------- | ----- | --------- |
| els | that not | only | align closely | with | the | true objec- |     |              |         |         |               |        |            |       |           |
|     |          |      |               |      |     |             |     | environments | [Towers |         | et al.,       | 2024], | they are   | often | absent or |
tivesbutalsofacilitatepolicyoptimization. Inthis difficulttospecifyinmanyreal-worldapplications.Inlightof
survey, we provide a comprehensive review of re- this, asignificantportionofmodernRLresearchfocuseson
| ward     | modeling      | techniques |              | within    | the deep       | RL lit-  |     |             |                   |          |         |            |         |       |            |
| -------- | ------------- | ---------- | ------------ | --------- | -------------- | -------- | --- | ----------- | ----------------- | -------- | ------- | ---------- | ------- | ----- | ---------- |
|          |               |            |              |           |                |          |     | how to      | extract effective |          | rewards | from       | various | types | of feed-   |
| erature. |               | We begin   | by outlining |           | the background |          |     |             |                   |          |         |            |         |       |            |
|          |               |            |              |           |                |          |     | back, after | which             | standard | RL      | algorithms | can     | be    | applied to |
| and      | preliminaries |            | in reward    | modeling. |                | Next, we |     |             |                   |          |         |            |         |       |            |
optimizethepoliciesofagents.
presentanoverviewofrecentrewardmodelingap- Despite the crucial role of reward modeling in RL, exist-
proaches, categorizing them based on the source, ingsurveys[AroraandDoshi,2021;Kaufmannetal.,2023]
| themechanism,andthelearningparadigm. |         |                |     |            |         | Build- |     |           |          |        |          |     |            |      |        |
| ------------------------------------ | ------- | -------------- | --- | ---------- | ------- | ------ | --- | --------- | -------- | ------ | -------- | --- | ---------- | ---- | ------ |
|                                      |         |                |     |            |         |        |     | are often | embedded | within | specific |     | subdomains | such | as in- |
| ing                                  | on this | understanding, |     | we discuss | various | ap-    |     |           |          |        |          |     |            |      |        |
versereinforcementlearning(IRL)andreinforcementlearn-
plicationsoftheserewardmodelingtechniquesand
|        |         |     |            |        |         |     |     | ing from | human    | feedback        | (RLHF), |        | with a | limited | focus on  |
| ------ | ------- | --- | ---------- | ------ | ------- | --- | --- | -------- | -------- | --------------- | ------- | ------ | ------ | ------- | --------- |
| review | methods | for | evaluating | reward | models. | Fi- |     |          |          |                 |         |        |        |         |           |
|        |         |     |            |        |         |     |     | reward   | modeling | as a standalone |         | topic. | To     | bridge  | this gap, |
nally, we conclude by highlighting promising re- we provide a systematic review of reward models, cover-
| search | directions |     | in reward | modeling. |     | Altogether, |     |           |              |     |                    |     |     |                  |     |
| ------ | ---------- | --- | --------- | --------- | --- | ----------- | --- | --------- | ------------ | --- | ------------------ | --- | --- | ---------------- | --- |
|        |            |     |           |           |     |             |     | ing their | foundations, |     | key methodologies, |     |     | and applications |     |
thissurveyincludesbothestablishedandemerging
|     |     |     |     |     |     |     |     | across diverse | RL  | settings. | We  | introduce | a   | new categoriza- |     |
| --- | --- | --- | --- | --- | --- | --- | --- | -------------- | --- | --------- | --- | --------- | --- | --------------- | --- |
methods,fillingthevacancyofasystematicreview
|     |     |     |     |     |     |     |     | tion framework |     | that addresses |     | three | fundamental |     | questions: |
| --- | --- | --- | --- | --- | --- | --- | --- | -------------- | --- | -------------- | --- | ----- | ----------- | --- | ---------- |
ofrewardmodelsincurrentliterature.
|     |     |     |     |     |     |     |     | (1) The           | source: | Where                            | does   | the reward  | come      | from? | (2)     |
| --- | --- | --- | --- | --- | --- | --- | --- | ----------------- | ------- | -------------------------------- | ------ | ----------- | --------- | ----- | ------- |
|     |     |     |     |     |     |     |     | The mechanism:    |         | What                             | drives | the agent’s | learning? |       | (3) The |
|     |     |     |     |     |     |     |     | learningparadigm: |         | Howtolearntherewardmodelfromvar- |        |             |           |       |         |
1 Introduction
|                |              |          |               |           |          |                 |        | ioustypesoffeedback? |                |       | Furthermore,wehighlightrecentad- |          |               |                 |         |
| -------------- | ------------ | -------- | ------------- | --------- | -------- | --------------- | ------ | -------------------- | -------------- | ----- | -------------------------------- | -------- | ------------- | --------------- | ------- |
|                |              |          |               |           |          |                 |        | vancements           | in reward      |       | models                           | based    | on foundation |                 | models, |
| In recent      | years,       | deep     | reinforcement |           | learning | (DRL),          | a ma-  |                      |                |       |                                  |          |               |                 |         |
|                |              |          |               |           |          |                 |        | such as              | large language |       | models                           | (LLMs)   | and           | vision-language |         |
| chine learning |              | paradigm | that combines |           | RL with  | deep            | learn- |                      |                |       |                                  |          |               |                 |         |
|                |              |          |               |           |          |                 |        | models               | (VLMs),        | which | have                             | received | relatively    | little          | atten-  |
| ing, has       | demonstrated |          | its immense   | potential |          | in applications |        |                      |                |       |                                  |          |               |                 |         |
across various domains. For example, AlphaGo [Silver et tioninprevioussurveys. Theframeworkofrewardmodeling
al., 2016] showcased RL’s capability of complex decision- weestablishinthissurveyisillustratedinFigure1. Specifi-
makingingamescenarios;InstructGPT[Ouyangetal.,2022] cally,thissurveyisorganizedasfollows:
| marked | the irreplaceable |          | role   | of RL | in aligning | language        |     |                                          |     |           |            |     |     |        |         |
| ------ | ----------------- | -------- | ------ | ----- | ----------- | --------------- | --- | ---------------------------------------- | --- | --------- | ---------- | --- | --- | ------ | ------- |
|        |                   |          |        |       |             |                 |     | 1. Backgroundofrewardmodeling(Section2). |     |           |            |     |     |        | Wefirst |
| models | with human        | intents; | agents |       | trained     | via large-scale |     |                                          |     |           |            |     |     |        |         |
|        |                   |          |        |       |             |                 |     | provide                                  | the | necessary | background |     | on  | RL and | reward  |
RL,suchasOpenAI-o1andDeepSeek-R1[Guoetal.,2025],
models;
| demonstrated | impressive |     | reasoning | intelligence |     | that is | com- |     |     |     |     |     |     |     |     |
| ------------ | ---------- | --- | --------- | ------------ | --- | ------- | ---- | --- | --- | --- | --- | --- | --- | --- | --- |
parableorevenexceedshumancapability. Unlikesupervised 2. Categorizationofrewardmodels. Weproposeaclas-
learning(SL)wheretheagentisrequiredtoimitateandrepli- sification framework for reward models, distinguishing
catethebehaviorrecordedinthedataset,RLsetsitselfapart them by three key factors: the source (Section 3), the

|     |     |     |     |     |     | Mechanisms |     |     |     |     | Reward 𝑟 |     |     |     |     |
| --- | --- | --- | --- | --- | --- | ---------- | --- | --- | --- | --- | -------- | --- | --- | --- | --- |
Agent
Sources
Intrinsic
Reward
Human-provided
|              |     |     |                   |                |     |     |           |     | RewardModel |     |     | State 𝑠 |     | Action 𝑎 |     |
| ------------ | --- | --- | ----------------- | -------------- | --- | --- | --------- | --- | ----------- | --- | --- | ------- | --- | -------- | --- |
| AI-generated |     |     | Learningparadigms |                |     |     |           |     |             |     |     |         |     |          |     |
|              |     |     |                   | Demonstrations |     |     | Extrinsic |     |             |     |     |         |     |          |     |
Reward
Goals
Environment
A≻𝐵 Preferences
Figure1:AframeworkforrewardmodelinginRL,categorizingrewardmodelsbytheirsources,feedbacktypes,andmechanismstoprovide
astructuredunderstandingofhowrewardsarederivedandutilizedinRLsystems.
mechanism
that drives learning (Section 4), and the carefully crafted by the task designer. This careful design
learning paradigm used to derive rewards (Section 5). iscrucialtoensurethatthespecifiedrewardstrulyreflectthe
We also list recent publications about reward modeling underlyingobjectives. Inmanyapplications,onlydescriptive
andcategorizethembasedonourhierarchyinTable1. guidelines or standards of the intended goals are available,
andthereforeweneedtoconvertthemintostatisticalreward
3. Applicationsandevaluationmethodsofrewardmod-
|                                                   |     |     |     |                      |     |     |     | models.        | Thisprocessistermedasrewardmodelingthrough- |     |     |     |     |     |     |
| ------------------------------------------------- | --- | --- | --- | -------------------- | --- | --- | --- | -------------- | ------------------------------------------- | --- | --- | --- | --- | --- | --- |
| els(Section6andSection7).                         |     |     |     | Weprovideadiscussion |     |     |     |                |                                             |     |     |     |     |     |     |
| ontheapplicationsofrewardmodelsinpracticalscenar- |     |     |     |                      |     |     |     | outthissurvey. |                                             |     |     |     |     |     |     |
ios,togetherwithevaluationmethodsforthesemodels.
|               |     |            |     |             |     |          |     | 3 SourcesofRewards |     |     |     |     |     |     |     |
| ------------- | --- | ---------- | --- | ----------- | --- | -------- | --- | ------------------ | --- | --- | --- | --- | --- | --- | --- |
| 4. Prosperous |     | directions | and | discussions |     | (Section | 8). |                    |     |     |     |     |     |     |     |
Inthissection,weexploredifferentsourcesofrewardsignals
Wesummarizethissurveybypresentingpotentialfuture
directionsinthistopic. in RL. We categorize reward sources into two main types:
|     |     |     |     |     |     |     |     | human-provided   |     | rewards, | which        | leverage | human    |       | expertise |
| --- | --- | --- | --- | --- | --- | --- | --- | ---------------- | --- | -------- | ------------ | -------- | -------- | ----- | --------- |
|     |     |     |     |     |     |     |     | and supervision, |     | and      | AI-generated |          | rewards, | which | rely on   |
2 Background
foundationmodelstypicallytrainedbyself-supervisedlearn-
RL is typically formulated as a Markov Decision Process ingoninternet-scaledatasets.
(MDP)⟨S,A,T,R,γ⟩,whereSandAdenotethestatespace
and the action space, respectively. The transition function 3.1 Human-ProvidedRewards
T(·|s,a)definesthedistributionoverthenextstatesaftertak-
ManualRewardEngineering
ing action a at state s. The reward model R(s,a) specifies Manual reward engineering refers to the process where
theinstantaneousrewardthattheagentwillreceiveaftertak-
|            |            |     |        |                 |     |          |           | researchers                  | meticulously |     | design     | reward                  | functions   |     | to steer |
| ---------- | ---------- | --- | ------ | --------------- | --- | -------- | --------- | ---------------------------- | ------------ | --- | ---------- | ----------------------- | ----------- | --- | -------- |
| ing action | a at state | s,  | and γ  | is the discount |     | factor   | that bal- |                              |              |     |            |                         |             |     |          |
|            |            |     |        |                 |     |          |           | agentstowardoptimalpolicies. |              |     |            | TakethewalkertaskinGym- |             |     |          |
| ances the  | importance | of  | future | rewards.        | An  | RL agent | aims      |                              |              |     |            |                         |             |     |          |
|            |            |     |        |                 |     |          |           | MuJoCo                       | [Towers      | et  | al., 2024] | as                      | an example: | its | reward   |
tofindthepolicyπ(a|s)maximizingthefollowingexpected
|     |     |     |     |     |     |     |     | is manually |     | designed | as a combination |     | of survival, |     | forward |
| --- | --- | --- | --- | --- | --- | --- | --- | ----------- | --- | -------- | ---------------- | --- | ------------ | --- | ------- |
discountedcumulativereward(a.k.a. return): movement, and control cost penalties. However, reward en-
|     |     |     | (cid:34) |     |     | (cid:35) |     |                                                       |     |     |     |     |     |     |     |
| --- | --- | --- | -------- | --- | --- | -------- | --- | ----------------------------------------------------- | --- | --- | --- | --- | --- | --- | --- |
|     |     |     | ∞        |     |     |          |     | gineeringrequireshumanexpertstotranslateambiguoustask |     |     |     |     |     |     |     |
(cid:88)
J(π)=E γtR(s ,a ) . (1) objectives into precise statistical models. Such an undertak-
|     |     |     | π,T |     | t t |     |     |                                            |     |     |     |     |     |             |     |
| --- | --- | --- | --- | --- | --- | --- | --- | ------------------------------------------ | --- | --- | --- | --- | --- | ----------- | --- |
|     |     |     |     |     |     |     |     | ingcanbebothresource-intensiveandperilous: |     |     |     |     |     | ifthereward |     |
t=0
functionisinadequatelycrafted,theagentmaysufferfromre-
wheretheexpectationistakenoverthedistributionofstates wardhacking,leadingtounpredictablebehaviors[Kaufmann
| andactionsthattheagentwillencounterfollowingπandT. |     |           |     |             |     |       |           | etal.,2023]. |     |     |     |     |     |     |     |
| -------------------------------------------------- | --- | --------- | --- | ----------- | --- | ----- | --------- | ------------ | --- | --- | --- | --- | --- | --- | --- |
| The fundamental                                    |     | objective |     | of learning |     | is to | refine an |              |     |     |     |     |     |     |     |
agent’s behavior to accomplish predefined goals or tasks. Human-in-the-LoopRewardLearning
Whilesupervisedlearning(SL)offersaprincipledapproach Insteadofdirectlycraftingtherewardmodels,human-in-the-
looprewardlearningderivesrewardsfromindirecthumansu-
bytrainingagentsonhuman-annotateddatasetstomimichu-
manbehavior,thismethodislimitedbythequantityandqual- pervision, includingdemonstrations[AbbeelandNg,2004],
ityofavailablehumandemonstrations. Consequently,agents goals [Liu et al., 2022], and preferences [Kaufmann et al.,
trained solely by SL may make irrational decisions when 2023]. Comparedtomanualrewardengineering, askinghu-
human behavior is missing or sub-optimal. Reinforcement man experts to provide demonstrations or feedback of such
learning instead offers another principled way that permits kind is much more straightforward. However, the reward
theagenttoexploretheenvironmentautonomouslyandadapt learningprocessneedstobespecificallydesignedtoaccom-
its behavior based on the rewards it receives. Such a trial- modate different kinds of supervision and ensure alignment
and-errorapproachexemptstheagentfromtheconstraintsof withtheintendedtaskobjectives.
| datasets                       | and opens | the | possibility | of  | achieving | or  | even sur- |     |                     |     |     |     |     |     |     |
| ------------------------------ | --------- | --- | ----------- | --- | --------- | --- | --------- | --- | ------------------- | --- | --- | --- | --- | --- | --- |
| passinghuman-levelperformance. |           |     |             |     |           |     |           | 3.2 | AI-GeneratedRewards |     |     |     |     |     |     |
Although S,A, and the transition model T are inherently Foundation models, such as large language models (LLMs)
defined by the environment, the reward model R must be andvision-languagemodels(VLMs)pre-trainedoninternet-

scale human-generated data, have demonstrated a remark- [Pathaketal.,2019;Sekaretal.,2020],manymethodsquan-
able ability to interpret human intent and autonomously de- tifythestrangenessofstatesasthepredictionerrorsofstate
fine reward models for RL. For instance, LLMs have been transition, and thus use the errors as intrinsic rewards to en-
employed to design reward functions [Xie et al., 2023] and couragetheagenttoexploreunseenareasoftheenvironment.
generatefeedbackforrewardlearning[Klissarovetal.,2023; Thestrangenessofstatescanalsobequantifiedusingthedis-
Bai et al., 2022; Lee et al., 2024]. VLMs, in particular, are tillationerrorbetweenrandomlyinitializednetworks[Burda
highlyeffectiveinspecifyingrewardsandtaskswithinvisu- etal.,2018],whichcanbemoreflexibletoimplement.
ally complex environments. Some studies [Fan et al., 2022; Other works design intrinsic rewards for exploration
Sontakke et al., 2023] compute semantic similarity between throughthelensofdatadiversity. Amongthem,count-based
agentstatesandtaskdescriptions,enablingdenserewardsig- methods, such as the well-known upper confidence bound
| nalsfromvisualobservations. |     |     |     | Others[Wangetal.,2024]uti- |     |     |     |       |                   |     |        |          |           |         |
| --------------------------- | --- | --- | --- | -------------------------- | --- | --- | --- | ----- | ----------------- | --- | ------ | -------- | --------- | ------- |
|                             |     |     |     |                            |     |     |     | (UCB) | [Lai and Robbins, |     | 1985], | maintain | the state | visita- |
lizeVLMstoanalyzevisualinputsandgeneratepreference-
tioncountsandassignhigherintrinsicrewardsforless-visited
basedfeedbackforrewardmodeltraining. Whilecertainap- states.Later,statichashing[Tangetal.,2017]anddensityes-
proaches[Baumlietal.,2023]leverageoff-the-shelffounda- timation [Bellemare et al., 2016; Ostrovski et al., 2017] are
tionmodelsforzero-shotrewardspecification,others[Fanet incorporated to extend count-based exploration to problems
al., 2022; Sontakke et al., 2023] fine-tune these models on with larger or even continuous state spaces. On the other
domain-specificdatasetstoimproverewarddesign.
hand,LiuandAbbeel[2021]andBadiaetal.[2020]promote
diversitybyestimatingthedataentropyandusingtheentropy
4 RewardMechanisms astheintrinsicrewards. Inthisway, theycanencouragethe
agenttoexplorenovelanddiversestates.
Inthissection,wefocusontwodifferentrewardmechanisms
thatdriveRLagent’slearning.
Empowerment
4.1 ExtrinsicReward
|     |     |     |     |     |     |     |     | Empowerment, | an  | information-theoreticintrinsicmotivation |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | ------------ | --- | ---------------------------------------- | --- | --- | --- | --- |
Rewards are defined by incentives that drive the agent. The (IM) concept, motivates an agent to maximize its influence
term extrinsic reward corresponds to incentives that arise on the environment by seeking states where it possesses the
| from external | sources |     | and directly | relate | to  | the desired | task |     |     |     |     |     |     |     |
| ------------- | ------- | --- | ------------ | ------ | --- | ----------- | ---- | --- | --- | --- | --- | --- | --- | --- |
greatestcontroloverfutureoutcomes[Klyubinetal.,2005].
objective,e.g.,instructionsorgoalssetbysupervisorsorem-
Anintrinsicrewardsignalcanthenbeformulatedtoguidethe
ployers. Definingextrinsicrewardsrequiresthetaskdesigner agent’s exploration towards states that offer greater control
totranslateabstractgoalsintoconcrete,quantifiablerewards andawiderdiversityofachievableconsequences. Manypre-
that can be incorporated into a standard RL pipeline. The vious works leverage empowerment for skill discovery [Ey-
approachtoaccomplishthisisdetailedinSection5. et al., et al.,
|     |     |     |     |     |     |     |     | senbach | 2018;                    | Mazzaglia |        | 2022].   | These | works |
| --- | --- | --- | --- | --- | --- | --- | --- | ------- | ------------------------ | --------- | ------ | -------- | ----- | ----- |
|     |     |     |     |     |     |     |     | aim to  | find a skill-conditioned |           | policy | π(a|s,z) | that  | maxi- |
4.2 IntrinsicMotivation
mizesthemutualinformationbetweentheresultingtrajectory
Incontrasttoextrinsicrewards,intrinsicmotivation(IM)cap- and the latent variable z. The intrinsic reward is designed
turesanagent’sinnatemotivationtoexploreandrefineitsbe- basedonthedecompositionofthismutualinformation. The
havior in the environment [Ryan and Deci, 2000]. Harlow agentisthenencouragedtorecoverthelatentz fromthetra-
| [1950] observed |     | that even | without | extrinsic |     | stimulus, | mon- |          |          |                |     |                |     |            |
| --------------- | --- | --------- | ------- | --------- | --- | --------- | ---- | -------- | -------- | -------------- | --- | -------------- | --- | ---------- |
|                 |     |           |         |           |     |           |      | jectory, | implying | that different | z   | should produce |     | distinctly |
keys have spontaneous desire and curiosity to solve com- differenttrajectories,therebydefiningz astheskill. Bypro-
plex puzzles. Later [Barto et al., 2004] introduced IM into viding an intrinsic reward based on the agent’s potential to
the reward mechanism, leading to the application of intrin- influence the environment, skill learning through empower-
sic reward. Unlike extrinsic rewards, intrinsic rewards are ment enables more generalizable agent behaviors and facili-
| oftendisentangledfromspecifictaskobjectives; |     |     |     |     |     | rather, | they |     |     |     |     |     |     |     |
| -------------------------------------------- | --- | --- | --- | --- | --- | ------- | ---- | --- | --- | --- | --- | --- | --- | --- |
tatesrapidadaptationtonewtasks.
| encapsulate                        | the | encouragement |     | for beneficial |     | behaviors | for |                    |     |     |     |     |     |     |
| ---------------------------------- | --- | ------------- | --- | -------------- | --- | --------- | --- | ------------------ | --- | --- | --- | --- | --- | --- |
| problem-solving,suchasexploration. |     |               |     |                |     |           |     | Knowledge-DrivenIM |     |     |     |     |     |     |
Tocoordinatetheintrinsicrewardandextrinsicreward,one
|        |          |     |            |     |         |        |        | Many approaches |     | leverage | high-level | knowledge | and | struc- |
| ------ | -------- | --- | ---------- | --- | ------- | ------ | ------ | --------------- | --- | -------- | ---------- | --------- | --- | ------ |
| common | approach | is  | to compute | the | agent’s | reward | r as a |                 |     |          |            |           |     |        |
weightedsumoftheintrinsicrewardr andtheextrinsicre- turedreasoningtogenerateintrinsicrewards,bridgingthegap
int
wardr : between abstract understanding and low-level sensorimotor
ext
|     |     |     |     |     |     |     |     | interactions. | Some | methods | derive | preferences | from | struc- |
| --- | --- | --- | --- | --- | --- | --- | --- | ------------- | ---- | ------- | ------ | ----------- | ---- | ------ |
r =λr +(1−λ)r , (2) tured event descriptions, comparing pairs of observations to
|         |       |        | int         |      | ext      |     |           |                                                       |     |     |     |     |     |     |
| ------- | ----- | ------ | ----------- | ---- | -------- | --- | --------- | ----------------------------------------------------- | --- | --- | --- | --- | --- | --- |
|         |       |        |             |      |          |     |           | infermeaningfulintrinsicsignals[Klissarovetal.,2023]. |     |     |     |     |     | Xu  |
| where 0 | ≤ λ ≤ | 1 is a | coefficient | that | balances | the | intrinsic |                                                       |     |     |     |     |     |     |
etal.[2023]adoptedareward-shapingtechniquebytreating
| rewardr | andextrinsicrewardr |     |     | .   |     |     |     |          |               |       |           |              |     |         |
| ------- | ------------------- | --- | --- | --- | --- | --- | --- | -------- | ------------- | ----- | --------- | ------------ | --- | ------- |
|         | int                 |     |     | ext |     |     |     |          |               |       |           |              |     |         |
|         |                     |     |     |     |     |     |     | valuable | propositional | logic | knowledge | as intrinsic |     | rewards |
Next,weintroducethreewidelyusedtypesofintrinsicmo-
|     |     |     |     |     |     |     |     | for the RL | procedure. | Du  | et al. [2023] | generates | goal | can- |
| --- | --- | --- | --- | --- | --- | --- | --- | ---------- | ---------- | --- | ------------- | --------- | ---- | ---- |
tivationinreinforcementlearning.
|     |     |     |     |     |     |     |     | didates | based on an | agent’s | current | context | and provides | re- |
| --- | --- | --- | --- | --- | --- | --- | --- | ------- | ----------- | ------- | ------- | ------- | ------------ | --- |
Exploration wardsforachievingthoseinferredobjectives. Inrecentwork
IM has long been used to encourage exploration. By lever- [Klissarovetal.,2023],large-scalemodelssuchasLLMsand
aging concepts such as surprise [Pathak et al., 2017], epis- VLMs have been employed to facilitate this process due to
temicuncertainty[Houthooftetal.,2016],anddisagreement theirbroadknowledgeandreasoningcapabilities.

Source Mechanism Feedback Method
[Pathaketal.,2017;Houthooftetal.,2016;Pathaketal.,2019;Sekaretal.,
2020;Burdaetal.,2018;Bellemareetal.,2016;Badiaetal.,2020;Liuand
human intrinsic -
Abbeel, 2021; Eysenbach et al., 2018; Mazzaglia et al., 2022; Wan et al.,
2024]
AI intrinsic - [Klissarovetal.,2023;Xuetal.,2023;Duetal.,2023]
[Abbeel and Ng, 2004; Ziebart et al., 2008; Finn et al., 2016a,b; Fu et al.,
human extrinsic demonstration
2017;Jeonetal.,2020]
[Liuetal.,2022;Nachumetal.,2018;Mazzagliaetal.,2024;Hartikainen
human extrinsic goal et al., 2019; Mendonca et al., 2021; Park et al., 2023; Myers et al., 2024;
Wangetal.,2025]
AI extrinsic goal [Sontakkeetal.,2023;Fanetal.,2022;Rocamondeetal.,2023]
[Christianoetal.,2017;Kimetal.,2023;VermaandMetcalf,2024;Knoxet
human extrinsic preference al.,2022;Touvronetal.,2023;Liuetal.,2024a;Ouyangetal.,2022;Ko¨pf
etal.,2023;Rafailovetal.,2023;Songetal.,2024;Liuetal.,2024b]
AI extrinsic preference [Baietal.,2022;Leeetal.,2024;Wangetal.,2024]
Table1:SummaryofthealgorithmsmentionedinSection3,Section4,andSection5.
5 LearningParadigms drawnfromtheBoltzmanndistribution:
exp(R (τ))
In this section, we focus on the paradigms of learning the p (τ)= θ , (3)
reward model R from different kinds of human feedback. θ Z
θ θ
Specifically, existing literature that involves reward learning where τ = (s ,a ,...,s ,a ) denotes the demonstrated
1 1 |τ| |τ|
canbebroadlycategorizedintothreeparadigms,namely:
trajectory, and R (τ) =
(cid:80)|τ|
R (s ,a ) is the cumulative
θ t=1 θ t t
• Learningfromdemonstrations,whichextractsreward rewardalongτ. ThepartitionfunctionZ normalizesthedis-
θ
modelsbasedondemonstrationsprovidedbyhumanex- tribution,anditcanbecomputedviadynamicprogramming
perts. This is related to inverse RL (IRL) [Arora and in small, discrete domains [Ziebart et al., 2008] or approxi-
Doshi,2021]. matedbyimportancesamplingincontinuoussettings[Finnet
al.,2016b].ByparameterizingtherewardmodelR aslinear
• Learning from goals, which derives reward models θ
models or neural networks, we can perform maximum like-
from specified goal states. This is related to goal-
lihoodtrainingbasedonobserveddemonstrationsandobtain
conditionalRL(GCRL)[Liuetal.,2022].
therewardmodelsthatexplainthedemonstrations.
• Learning from preferences, which extracts reward
AdversarialRewardLearning
modelsfromhumanpreferencesamongtwoormoretra-
Finnetal.[2016a]demonstratedthattheMaxEnt-IRLprob-
jectorysegments.Thisisrelatedtopreference-basedRL
lemcanbereformulatedasagenerativeadversarialnetwork
(PbRL) and reinforcement learning from human feed-
(GAN) problem by employing a specifically structured dis-
back(RLHF)[Kaufmannetal.,2023].
criminator. Let the generator of the trajectories and the re-
In each subsection, we will provide a brief overview of the wardmodelbeq (τ)andR (τ)respectively,thediscrimina-
ψ θ
establishedmethodsineachsetting. torisparameterizedas:
1 exp(R (τ))
5.1 LearningfromDemonstrations D (τ)= Z θ , (4)
θ 1 exp(R (τ))+q (τ)
Maximum-EntropyInverseReinforcementLearning Z θ ψ
Previous approaches to IRL iteratively optimize the reward where Z represents the partition function and can be esti-
modeltomaximizetheperformancemarginbetweendemon- mated via importance sampling. The generator and the dis-
strations and any other policy, such that the demonstrations criminatoraretrainedviastandardGANlosses:
appearoptimalunderthelearnedrewardmodel[Abbeeland L(θ)=E [−logD (τ)]+E [−log(1−D (τ))],
τ∼De θ τ∼q θ
Ng,2004]. However,theIRLproblemisinherentlyill-posed, (cid:20) (cid:21)
(1−D (τ))
because multiple distinct rewards may explain the same ex- L(ψ)=E log θ
τ∼qψ D (τ)
pertbehavior. Acommonstrategyforresolvingthisambigu- θ
ity is to incorporate additional regularization into the learn- =E [−R (τ)]−H(q )+logZ,
τ∼qψ θ ψ
ing objective. As an example, the maximum-entropy IRL (5)
(MaxEnt-IRL) framework [Ziebart et al., 2008] introduces where D denotes the expert demonstrations and H is the
e
entropyregularizationsuchthattheexpertdemonstrationsare entropy. By optimizing (5), we can effectively optimize

the reward model R . When the optimization converges, guided toward states that are in the proximity of the goal.
θ
it follows from the maximum-entropy theory that q∗(τ) ∝ Moreover,[Parketal.,2023]framestemporaldistancelearn-
exp(R∗(τ)), which exactly recovers the MaxEnt-IRL prob- ingasaconstrainedoptimizationproblem,maintainingadis-
lem in (3). However, conducting optimization over the tra- tancethresholdbetweenadjacentstateswhiledispersingoth-
jectories incurs high variance, and therefore the adversarial ers.Recently,[Myersetal.,2024]definesatemporaldistance
inverse RL (AIRL) framework [Fu et al., 2017] further de- metric based on successor features and temporal contrastive
composestheproblemandoperatesonastate-actionlevel: learning,whichisshowntosatisfythequasi-metricproperty.
Temporal distance offers a more grounded reward signal by
L(θ)=E [−logD (s,a)]+E [−log(1−D (s,a))],
De θ qψ θ effectivelyreflectingtheagent’sprogresstowardthegoaland
(6)
capturingdeepertasksemanticsbeyondvisualdetails.
whereD (s,a)= expfθ(s,a) .Oncetrainingiscom-
θ exp(fθ(s,a))+pψ(a|s) SemanticSimilarity
plete, f is shown to recover the optimal advantage func-
θ Semantic similarity-based rewards measure how closely the
tionA∗,fromwhichrewardmodelsmaysubsequentlybeex-
agent’scurrentstatealignswithagivengoalinasharedrep-
tracted.Buildingonthisfoundation,theAIRLframeworkhas
resentation space. RoboCLIP [Sontakke et al., 2023] com-
beenfurtherextended–forinstance,toencompassabroader
putestherewardasthedotproductbetweenthetextembed-
classofregularizations[Jeonetal.,2020].
dingofalanguage-specifiedgoalandthevideoembeddingof
theagent’sobservedtrajectory. MineCLIP[Fanetal.,2022]
5.2 LearningfromGoals
(cid:16) (cid:17)
computesrewardsasR = max P − 1 ,0 ,whereP is
Whenourintendedgoalscanbeexplicitlydescribedorspec- G NT G
ifiedasastateg ∈ S,therewardmodelcanbeconveniently the probability of the observation video matching the goal
defined based on whether the goal is achieved [Liu et al., description against negatives, and 1 serves as a baseline
NT
2022]: to filter out uncertain estimates. These embeddings can be
obtained from VLMs, which map multimodal inputs into a
R(s,g)=1(saccomplishesg), (7)
common space, allowing the agent to learn from high-level
where 1 is the indicator function. However, this binary re- instructionsordemonstrations.
ward structure is extremely sparse and inefficient for policy
5.3 LearningfromPreferences
optimization,becausetheagentonlyreceivesarewardupon
In many applications, obtaining human evaluations is com-
reachingthegoalstate,withoutintermediatesupervision. To
paratively cost-effective compared to collecting demonstra-
addressthissparsity,analternativesolutionistoreshapethe
tions or identifying the goal states. Consider training lan-
reward as the distance between the current and the desired
guage models to follow instructions as an example, it is
goal:
both tedious and time-consuming to require human annota-
R(s,g)=−d(ϕ(s),ψ(g)), (8) torstogeneratetemplateresponsesforeveryrequest. Onthe
contrary,comparingagent-generatedresponsesusingmetrics
whereϕandψaremappingfunctionsthattransformthestate
such as helpfulness, harmlessness, and truthfulness is con-
sandthegoalg tothesamelatentspace,andd(·,·)isaspe-
siderably more straightforward. In this section, we there-
cific distance metric on that space. This distance-based re-
fore investigate methods for deriving rewards from human-
ward provides a more nuanced measurement of the agent’s
annotatedpreferencesamongcandidateoptions.
progresstowardthespecifiedgoal. Inthebelow,wewillin-
Inthisframework,annotatorsareaskedtolabeltheirpref-
troducetwocommonlyadopteddistancemetrics: spatialdis- erencesy betweenapairoftrajectories(τ0,τ1), whereτ =
tanceandtemporaldistance. (s ,a ,...,s ,a ). A label y = 0 means τ0 is preferred
1 1 |τ| |τ|
SpatialDistance overτ1(denotedasτ0 ≻τ1),andy =1impliestheopposite.
Spatial distance directly quantifies the similarity between To build the connection between observed preferences and
states from the environment. Common approaches utilize reward models, we need preference models. A widely used
measuressuchastheL2distance[Nachumetal.,2018],and example is Bradley-Terry (BT) models [Bradley and Terry,
cosinesimilarity[Mazzagliaetal.,2024]toassesstheprox- 1952], which posit that the probability of preference can be
imity between states. These metrics may be computed ei- describedbyaBoltzmanndistributionappliedtothecumula-
ther in the raw state space [Nachum et al., 2018], or within tivereward:
c a a l p e t a u r r n e e s d an la d te e n x t p s lo p i a t c s e th [ e M p a r z o z b a l g e l m ia s e tr t u a c l t . u , r 2 e 0 . 24] which better P BT (τ0 ≻τ1;θ)= (cid:80) exp( (cid:80) ex ( p s ( 0 t (cid:80) ,a0 t )∈τ0 R θ (s R 0 t ,a ( 0 t s ) j ) ,aj)) .
j∈{0,1} (sj,aj)∈τj θ t t
TemporalDistance t t (9)
Otherworksfocusonthenotionoftemporaldistance,which TooptimizetherewardmodelR ,wecanmaximizethelike-
θ
conceptuallyassignshigherrewardstostatesthataretempo- lihoodoftheobservedpreferences:
rally closer to the goal state. For instance, approaches like L(θ)=−
Hartikainenetal.[2019]andWangetal.[2025]trainadis-
(cid:88)
tancemetricfunctiond ,suchthatd (s,g)approximatesthe (1−y)logP(τ0 ≻τ1;θ)+ylogP(τ1 ≻τ0;θ),
θ θ
number of time steps required for the agent to reach g from (τ0,τ1,y)∈D
s. Using R = −d as the reward model, the agent will be (10)
θ

where P is defined according to the preference model in extendingthecomparisontoK candidates:
| (9). After                                         | training, | we  | can label | the | reward of | each transi- |     |       |     |          |     |     |     |
| -------------------------------------------------- | --------- | --- | --------- | --- | --------- | ------------ | --- | ----- | --- | -------- | --- | --- | --- |
|                                                    |           |     |           |     |           |              |     | P (τ1 | ≻τ2 | ≻...≻τK) |     |     |     |
| tionpairandsubsequentlyemployanyRLalgorithmtoopti- |           |     |           |     |           |              |     | PL    |     |          |     |     |     |
m i ze t h e p o li c i e s [ C hr i s tia n o e ta l . , 2 0 1 7 ]. A l te r n a ti v e l y , w e K (cid:80) R(sk ,ak
|     |     |     |     |     |     |     |     | (cid:89) | exp( |     |     |     | )) (13) |
| --- | --- | --- | --- | --- | --- | --- | --- | -------- | ---- | --- | --- | --- | ------- |
ca n al s o d i re c t l y tr a in t h e p o lic y v i a ( 1 0 ) b y re p a r a m e t e r iz i ng = (sk t ,ak t )∈τk t t ,
|     |     |     |     |     |     |     |     |     | (cid:80)K | (cid:80) |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --------- | -------- | --- | --- | --- |
therewardmodelthroughthepolicyincertaincircumstances exp( R(sj,aj))
|                       |     |     |     |     |     |     |       | k=1   | j=k  |     | (sj,aj)∈τj | t               | t     |
| --------------------- | --- | --- | --- | --- | --- | --- | ----- | ----- | ---- | --- | ---------- | --------------- | ----- |
| [Rafailovetal.,2023]. |     |     |     |     |     |     |       |       |      |     | t t        |                 |       |
|                       |     |     |     |     |     |     | where | (τ1 ≻ | τ2 ≻ | ... | ≻ τK)      | is the observed | rank- |
PreferenceModels
ing. Substituting(13)into(10)yieldstheobjectiveoflearn-
Despite its popularity in PbRL literature, BT models may ing rewards from rankings [Rafailov et al., 2023; Song et
notalignwithreality[Kimetal.,2023]. Consequently,sev- al., 2024]. Another straightforward approach to rankings is
eralstudieshaveproposedalternativepreferencemodelsthat
|     |     |     |     |     |     |     | breaking | the ranking |     | into pairs | by selecting | two | candidates |
| --- | --- | --- | --- | --- | --- | --- | -------- | ----------- | --- | ---------- | ------------ | --- | ---------- |
morecloselyreflectthemechanismsunderlyinghumanpref- fromthelistandassigningthelabelaccordingtotheirranks,
erences. Preference Transformer [Kim et al., 2023] intro- thereby reducing the problem of applying BT models to all
duces importance weights over state-action pairs to account possible pairwise comparisons [Ouyang et al., 2022; Liu et
| forthedependenceoncertaincriticalstatesinthetrajectory: |     |     |     |     |     |     | al.,2024b]. |     |     |     |     |     |     |
| ------------------------------------------------------- | --- | --- | --- | --- | --- | --- | ----------- | --- | --- | --- | --- | --- | --- |
(cid:80)
|       |         | exp(     |          |          | w0R (s0,a0)) |          |                |     |     |     |     |     |     |
| ----- | ------- | -------- | -------- | -------- | ------------ | -------- | -------------- | --- | --- | --- | --- | --- | --- |
|       |         |          | (s0      | ,a0 )∈τ0 | t θ          | t t      | 6 Applications |     |     |     |     |     |     |
| P (τ0 | ≻τ1;θ)= |          |          | t t      |              | ,        |                |     |     |     |     |     |     |
| PT    |         | (cid:80) | (cid:80) |          | wjR          | (sj,aj)) |                |     |     |     |     |     |     |
|       |         |          | exp(     | (sj ,aj  | θ            |          |                |     |     |     |     |     |     |
j t t )∈τj t t t Reward model designing constitutes an indispensable step
(11)
|     |     |     |     |     |     |     | before | any practical | applications |     | of  | RL. Therefore, | in this |
| --- | --- | --- | --- | --- | --- | --- | ------ | ------------- | ------------ | --- | --- | -------------- | ------- |
∈{0,1}andtheweightswjaretheaverageattention
wherej
t section, we briefly review successful applications of reward
weightsofthepair(sj,aj)∈τjcalculatedbyabi-directional
t t models in deep RL, including control problems, generative
attentionlayer.Similarly,VermaandMetcalf[2024]replaced
modelfinetuning,andotherfields.
| weights | in (11) | with attention |     | weights | from a | transformer- |     |     |     |     |     |     |     |
| ------- | ------- | -------------- | --- | ------- | ------ | ------------ | --- | --- | --- | --- | --- | --- | --- |
based transition model, thereby incorporating state impor- 6.1 ControlProblems
| tance priors | from | the perspective |     | of transition | models. | Be- |     |     |     |     |     |     |     |
| ------------ | ---- | --------------- | --- | ------------- | ------- | --- | --- | --- | --- | --- | --- | --- | --- |
sides,theregret-basedmodels[Knoxetal.,2022]proposeto Reward models play a pivotal role in control problems, as a
modelhumanpreferencesbythesumofoptimaladvantages fundamental mechanism for guiding decision-making in dy-
|     |     |     |     |     |     |     | namic environments. |     |     | Christiano | et al. | [2017] | demonstrated |
| --- | --- | --- | --- | --- | --- | --- | ------------------- | --- | --- | ---------- | ------ | ------ | ------------ |
alongthetrajectory,ratherthantherewards:
|     |     |     |     |     |     |     | their effectiveness |     | in  | facilitating | policy | learning | across di- |
| --- | --- | --- | --- | --- | --- | --- | ------------------- | --- | --- | ------------ | ------ | -------- | ---------- |
exp(−Regret(τ0))
(τ0 ≻τ1)= versedomains,includinggame-playingandsimulatedcontin-
| P   |     |     |     |     |     | ,   |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Reg exp(−Regret(τ0))+exp(−Regret(τ1)) uouscontroltasks. Ingameplayscenarios, Fanetal.[2022]
leveragedgeneratedrewardstoenhancelearninginMinecraft
|τ|
(cid:88) tasks. In robotics, Sontakke et al. [2023] employed reward
| Regret(τ)= |     | [Q∗(s | ,a  | )−V∗(s | )], |     |     |     |     |     |     |     |     |
| ---------- | --- | ----- | --- | ------ | --- | --- | --- | --- | --- | --- | --- | --- | --- |
R t t R t models to train agents across various robotic tasks. Simi-
t=1 larly, in autonomous driving, the design of reward functions
(12)
| V∗                                              | Q∗  |       |             |       |       |              | remainsacriticalaspectoftrainingintelligentagents[Knox |     |     |     |     |     |     |
| ----------------------------------------------- | --- | ----- | ----------- | ----- | ----- | ------------ | ------------------------------------------------------ | --- | --- | --- | --- | --- | --- |
| with                                            | and | being | the optimal | state | value | function and |                                                        |     |     |     |     |     |     |
| R                                               |     | R     |             |       |       |              | etal.,2023].                                           |     |     |     |     |     |     |
| Q-valuefunctionfortherewardmodelR,respectively. |     |       |             |       |       | Knox         |                                                        |     |     |     |     |     |     |
etal.[2022]demonstratedthatthisapproachmaybetterpre-
6.2 GenerativeModelPost-training
dictrealhumanpreferenceandthelearnedrewardmodelmay
achievesuperiorperformanceinpractice.
Moderngenerativemodelstypicallyfeatureatwo-stagetrain-
ingprocedure,wherethepre-trainingstageinvolvesunsuper-
ExtensiontoOrdinalFeedback
|     |     |     |     |     |     |     | vised learning |     | on internet-scale |     | data, | and the | post-training |
| --- | --- | --- | --- | --- | --- | --- | -------------- | --- | ----------------- | --- | ----- | ------- | ------------- |
Ordinal feedback generalizes binary feedback by requiring stage fine-tunes the models and fits them for downstream
annotatorstoadditionallyspecifythestrengthsoftheirpref- tasks. A prominent example is InstructGPT [Ouyang et al.,
| erences(e.g., | slightlybetter |     | orsignificantlybetter). |     |     | Tointe- |        |               |     |       |          |       |               |
| ------------- | -------------- | --- | ----------------------- | --- | --- | ------- | ------ | ------------- | --- | ----- | -------- | ----- | ------------- |
|               |                |     |                         |     |     |         | 2022], | which employs |     | RL to | optimize | model | outputs based |
grate this more nuanced information, existing studies mod- on human preference data. Specifically, it trains a reward
ifyBTmodelsbyincorporatingsoftmargins[Touvronetal., model on human-ranked responses and fine-tunes the lan-
2023]orsoftlabelsy ∈ [0,1][Liuetal.,2024a],wherethe guagemodeltomaximizethisreward. Thisapproachhasbe-
i
marginorthelabelreflectsthestrengthofthepreference. comeastandardmethodforenhancingthehelpfulness,harm-
|     |     |     |     |     |     |     | lessness | [Dai | et al., 2023], |     | and general | task-solving | capa- |
| --- | --- | --- | --- | --- | --- | --- | -------- | ---- | -------------- | --- | ----------- | ------------ | ----- |
BeyondPairwiseComparisons
|     |     |     |     |     |     |     | bilities | of LLMs | [Abramson |     | et al., 2022]. | In  | mathematical |
| --- | --- | --- | --- | --- | --- | --- | -------- | ------- | --------- | --- | -------------- | --- | ------------ |
Human feedback can also be provided in the form of rank- problem-solving, goldenrewardscanbedefinedbycompar-
ingsamongmultiplecandidates[Ouyangetal.,2022;Ko¨pfet ing the model-generated answers with ground-truth answers
al.,2023]. Althoughsuchlistwisecomparisonsputagreater [Luongetal.,2024]orbyverifyingthecorrectnessusingfor-
burdenonannotators,theyalsocarryricherinformationthan mal solvers [Xin et al., 2024]. Some works also use LLM-
pairwise comparisons. To accommodate rankings, Plackett- basedverifiers[Zhangetal.,2024],furtherleveragingthein-
Luce(PL)models[Plackett,1975]generalizeBTmodelsby contextlearningabilityprovidedbyLLMs.

6.3 OtherFields 7.3 EvaluationviaInterpretableRepresentations
Inrecommendationsystems,Xueetal.[2023]trainedreward Although the evaluation of reward models may not be
models to allow RL recommendation systems to learn from straightforward,wecantransformthemequivalentlyintoin-
| users’ historical |     | behaviors. | Kim | and | Lee [2020] | used | a re- |             |                  |     |        |     |            |        |      |
| ----------------- | --- | ---------- | --- | --- | ---------- | ---- | ----- | ----------- | ---------------- | --- | ------ | --- | ---------- | ------ | ---- |
|                   |     |            |     |     |            |      |       | terpretable | representations. |     | Jenner |     | and Gleave | [2022] | pro- |
ward model to automate peer-to-peer (P2P) energy trading, posedtotransformrewardmodelswithpotential-basedshap-
while Oueida et al. [2019] designed a reward model to im- ingandvisualizetheshapedrewardinstead. Sincepotential-
provethemanagementofhealthcareresources. based shaping preserves the optimal policy, characteristics
|     |     |     |     |     |     |     |     | of the shaped | reward | may | also | apply | to the | original | reward |
| --- | --- | --- | --- | --- | --- | --- | --- | ------------- | ------ | --- | ---- | ----- | ------ | -------- | ------ |
7 EvaluatingRewardModels model. Alternatively, we can evaluate the reward model
|     |     |     |     |     |     |     |     | through | the behavior | of  | the | induced | policy | [Rocamonde | et  |
| --- | --- | --- | --- | --- | --- | --- | --- | ------- | ------------ | --- | --- | ------- | ------ | ---------- | --- |
Oncerewardmodelsaredeveloped,reliableevaluationtech-
al.,2023].
| niques are | essential | for           | comparing |          | or selecting | models | for     |               |     |     |     |     |     |     |     |
| ---------- | --------- | ------------- | --------- | -------- | ------------ | ------ | ------- | ------------- | --- | --- | --- | --- | --- | --- | --- |
| downstream | policy    | optimization. |           | However, |              | due to | the am- |               |     |     |     |     |     |     |     |
|            |           |               |           |          |              |        |         | 8 Conclusions |     |     |     |     |     |     |     |
biguouslinkbetweenrewardmodelsandfinalpolicyperfor-
mance,relyingonasingleevaluationperspectiveisoftenin- Recently, reward models have become a highly motivating
sufficient[AroraandDoshi,2021]. Wecategorizecommonly area of research, driven by both theoretical challenges and
| used reward | evaluation |     | techniques |     | into the | following | three |                                     |     |     |     |     |                  |     |     |
| ----------- | ---------- | --- | ---------- | --- | -------- | --------- | ----- | ----------------------------------- | --- | --- | --- | --- | ---------------- | --- | --- |
|             |            |     |            |     |          |           |       | practicalneedsacrossvariousdomains. |     |     |     |     | Weconsiderthede- |     |     |
types,whichareoftenusedincombinationtoachieveamore
|     |     |     |     |     |     |     |     | velopment | of reward | models |     | as a significant |     | step before | the |
| --- | --- | --- | --- | --- | --- | --- | --- | --------- | --------- | ------ | --- | ---------------- | --- | ----------- | --- |
comprehensiveassessmentofrewardmodels.
|     |     |     |     |     |     |     |     | application | of    | RL to real-world |          | problems, |      | and we hope | this |
| --- | --- | --- | --- | --- | --- | --- | --- | ----------- | ----- | ---------------- | -------- | --------- | ---- | ----------- | ---- |
|     |     |     |     |     |     |     |     | survey can  | offer | valuable         | insights | for       | both | researchers | and  |
7.1 EvaluationviaPolicyPerformance
|        |       |         |        |           |     |           |     | practitioners. | Although |        | our study  | provides |            | a comprehensive |        |
| ------ | ----- | ------- | ------ | --------- | --- | --------- | --- | -------------- | -------- | ------ | ---------- | -------- | ---------- | --------------- | ------ |
| Reward | model | quality | can be | evaluated | by  | measuring | the |                |          |        |            |          |            |                 |        |
|        |       |         |        |           |     |           |     | overview       | of the   | topic, | the design | and      | variations | of              | reward |
performance of policies trained with it. Primary metrics in- modelsstillextendbeyondthescopeofthisdiscussion.Inter-
cludeground-truthreward,tasksuccessrate,andtrainingef- estedreaderscanalsorefertoothersurveypapers[Eschmann,
ficiency,withsuperiorrewardmodelsyieldinghighervalues 2021;Liuetal.,2022;AroraandDoshi,2021;Kaufmannet
across these measures. This approach is widely adopted in al.,2023]thatfocusonRLsubfieldscloselyrelatedtoreward
| reinforcement | learning |     | literature | to  | assess the | alignment | be- |     |     |     |     |     |     |     |     |
| ------------- | -------- | --- | ---------- | --- | ---------- | --------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
modeling.
tweenrewardmodelsandactualobjectives[Christianoetal.,
2017]. However, these metrics are sensitive to policy opti- 8.1 FutureDirections
| mization | algorithms | and | environmental |     | stochasticity, |     | poten- |     |     |     |     |     |     |     |     |
| -------- | ---------- | --- | ------------- | --- | -------------- | --- | ------ | --- | --- | --- | --- | --- | --- | --- | --- |
Efficientandaccuraterewardmodelingisavaluableresearch
| tially limiting | their | ability | to  | independently |     | reflect | the true |           |      |             |             |     |            |             |     |
| --------------- | ----- | ------- | --- | ------------- | --- | ------- | -------- | --------- | ---- | ----------- | ----------- | --- | ---------- | ----------- | --- |
|                 |       |         |     |               |     |         |          | direction | with | significant | application |     | prospects. | It combines |     |
performanceoftherewardmodelitself.
|     |     |     |     |     |     |     |     | increasingly | mature | technologies |        | such   | as             | large models | and      |
| --- | --- | --- | --- | --- | --- | --- | --- | ------------ | ------ | ------------ | ------ | ------ | -------------- | ------------ | -------- |
|     |     |     |     |     |     |     |     | diffusion    | models | with         | reward | design | and generation |              | in rein- |
7.2 EvaluationviaDistanceMetrics
forcementlearningtoprovidebehavioralfeedbackforagents
| To evaluate  | and      | compare | reward | models,         | another | approach |         |                |     |           |                  |     |     |                 |     |
| ------------ | -------- | ------- | ------ | --------------- | ------- | -------- | ------- | -------------- | --- | --------- | ---------------- | --- | --- | --------------- | --- |
|              |          |         |        |                 |         |          |         | in perception, |     | planning, | decision-making, |     |     | and navigation. |     |
| is to design | distance | metrics |        | that accurately |         | reflect  | the be- |                |     |           |                  |     |     |                 |     |
Althoughthereisnodefinitiveconclusiononwhichroutecan
havioral differences between the policies induced by these achieveefficientrewardmodeling, researchonvarioustech-
rewards. The pioneering work EPIC [Gleave et al., 2020] nologiesinrecentyearshaseffectivelypromotedthedevelop-
introducescanonicallyshapedrewardstoremoveambiguity ment of this field. With the continuous development of ma-
andinvariancesfromrewardmodelsandproposestousethe
|     |     |     |     |     |     |     |     | chine learning |     | and reinforcement |     | learning, |     | reward modeling |     |
| --- | --- | --- | --- | --- | --- | --- | --- | -------------- | --- | ----------------- | --- | --------- | --- | --------------- | --- |
Pearsoncoefficientbetweentwocanonicallyshapedrewards
|                                  |     |     |     |     |                    |     |     | has many | valuable | research | directions |     | in the | future, | includ- |
| -------------------------------- | --- | --- | --- | --- | ------------------ | --- | --- | -------- | -------- | -------- | ---------- | --- | ------ | ------- | ------- |
| asameasureoftherewardsimilarity. |     |     |     |     | TheEPICdistancebe- |     |     | ing:     |          |          |            |     |        |         |         |
tweentworewardmodelsisdemonstratedtoupper-boundthe
|                                                 |     |     |     |     |     |     |       | 1. Vectorized |     | rewards: | Constructing |     | vectorized |     | rewards |
| ----------------------------------------------- | --- | --- | --- | --- | --- | --- | ----- | ------------- | --- | -------- | ------------ | --- | ---------- | --- | ------- |
| performancedifferencebetweentheinducedpolicies. |     |     |     |     |     |     | Lower |               |     |          |              |     |            |     |         |
EPIC distances to the ground truth reward indicate superior to replace scalarized single rewards, dynamically bal-
rewardmodelingcapability. ancing multiple competitive reward signals to provide
BasedonEPIC,Wulfeetal.[2022]furtherincorporatesthe agentswithmorecomprehensivefeedback.
dynamicsinformationwhenconsideringtheinvariantreward 2. Interpretating reward models: Improving the trans-
shapingandintroducestheDARDmetric,whichismorepre- parencyofrewardfunctionsandexplainingthedecision-
dictiveandaccurateinquantifyingthedifferencesinrewards. makinglogicbehindrewardmodels.
Furthermore,Skalseetal.[2023]presentedageneralframe-
workfordesigningsuchdistancemetrics. TheSTARCmet- 3. Ethicalalignmentandsocialvalueconstraints:Quan-
|     |     |     |     |     |     |     |     | tifying | ethical | principles |     | and embedding |     | them | into re- |
| --- | --- | --- | --- | --- | --- | --- | --- | ------- | ------- | ---------- | --- | ------------- | --- | ---- | -------- |
ricsprovidedinthisframeworkareshowntoinduceboththe
wardfunctionswhileavoidingpotentialsideeffectsdur-
upperboundandthelowerboundoftheperformancediffer-
ingtheoptimizationprocess.
| ences, and | any other | metrics |     | that possess | the | same property |     |     |     |     |     |     |     |     |     |
| ---------- | --------- | ------- | --- | ------------ | --- | ------------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
must be equivalent to the STARC metrics up to bilipschitz 4. Reward foundation models: Similar to constructing a
scaling. When datasets containing ground-truth rewards are generalrepresentationspace,considertrainingafounda-
available,distancemetricsareparticularlysuitableforoffline tionrewardmodelthatcanobtaingeneralrewardvalues
evaluation,circumventingthenecessityforpolicylearning. basedondiverseinputs(suchaslimbmovements).

Acknowledgements pretraininginreinforcementlearningwithlargelanguagemodels.
InInternationalConferenceonMachineLearning,pages8657–
Wethanktheanonymousreviewersfortheirinsightfulfeed-
8677.PMLR,2023.
back. ThisworkwassupportedbytheNationalScienceand
JonasEschmann.Rewardfunctiondesigninreinforcementlearning.
| TechnologyMajorProject(GrantNo. |     |     |     | 2022ZD0114805)and |     |     |               |     |          |             |          |     |                   |     |
| ------------------------------- | --- | --- | --- | ----------------- | --- | --- | ------------- | --- | -------- | ----------- | -------- | --- | ----------------- | --- |
|                                 |     |     |     |                   |     |     | Reinforcement |     | learning | algorithms: | Analysis |     | and Applications, |     |
YoungScientistsFundoftheNationalNaturalScienceFoun-
pages25–33,2021.
| dationofChina(PhDCandidate)(GrantNo. |     |     |     |     | 624B200197). |     |          |                        |          |        |                           |        |     |        |
| ------------------------------------ | --- | --- | --- | --- | ------------ | --- | -------- | ---------------------- | -------- | ------ | ------------------------- | ------ | --- | ------ |
|                                      |     |     |     |     |              |     | Benjamin | Eysenbach,             | Abhishek | Gupta, | Julian                    | Ibarz, | and | Sergey |
|                                      |     |     |     |     |              |     | Levine.  | Diversityisallyouneed: |          |        | Learningskillswithoutare- |        |     |        |
References
|               |     |        |       |                |          |         | wardfunction. |     | arXivpreprintarXiv:1802.06070,2018. |     |     |     |     |     |
| ------------- | --- | ------ | ----- | -------------- | -------- | ------- | ------------- | --- | ----------------------------------- | --- | --- | --- | --- | --- |
| Pieter Abbeel | and | Andrew | Y Ng. | Apprenticeship | learning | via in- |               |     |                                     |     |     |     |     |     |
LinxiFan,GuanzhiWang,YunfanJiang,AjayMandlekar,Yuncong
| versereinforcementlearning. |     |     |     | InProceedingsofthetwenty-first |     |     |                 |     |             |     |             |     |          |     |
| --------------------------- | --- | --- | --- | ------------------------------ | --- | --- | --------------- | --- | ----------- | --- | ----------- | --- | -------- | --- |
|                             |     |     |     |                                |     |     | Yang, HaoyiZhu, |     | AndrewTang, |     | De-AnHuang, |     | YukeZhu, | and |
internationalconferenceonMachinelearning,page1,2004.
|     |     |     |     |     |     |     | AnimaAnandkumar. |     | Minedojo: |     | Buildingopen-endedembodied |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | ---------------- | --- | --------- | --- | -------------------------- | --- | --- | --- |
JoshAbramson,ArunAhuja,FedericoCarnevale, PetkoGeorgiev, agentswithinternet-scaleknowledge. AdvancesinNeuralInfor-
AlexGoldin,AldenHung,JessicaLandon,JirkaLhotka,Timothy mationProcessingSystems,35:18343–18362,2022.
Lillicrap,AlistairMuldal,etal.Improvingmultimodalinteractive
|                                                   |     |     |     |     |     |       | Chelsea Finn, | Paul    | Christiano, | Pieter | Abbeel,     | and | Sergey    | Levine. |
| ------------------------------------------------- | --- | --- | --- | --- | --- | ----- | ------------- | ------- | ----------- | ------ | ----------- | --- | --------- | ------- |
| agentswithreinforcementlearningfromhumanfeedback. |     |     |     |     |     | arXiv |               |         |             |        |             |     |           |         |
|                                                   |     |     |     |     |     |       | A connection  | between | generative  |        | adversarial |     | networks, | inverse |
preprintarXiv:2211.11602,2022.
reinforcementlearning,andenergy-basedmodels.arXivpreprint
SaurabhAroraandPrashantDoshi. Asurveyofinversereinforce- arXiv:1611.03852,2016.
| mentlearning: |     | Challenges,methodsandprogress. |     |     |     | ArtificialIn- |               |        |         |     |        |         |        |      |
| ------------- | --- | ------------------------------ | --- | --- | --- | ------------- | ------------- | ------ | ------- | --- | ------ | ------- | ------ | ---- |
|               |     |                                |     |     |     |               | Chelsea Finn, | Sergey | Levine, | and | Pieter | Abbeel. | Guided | cost |
telligence,297:103500,2021.
|     |     |     |     |     |     |     | learning: | Deep | inverse | optimal | control | via policy | optimization. |     |
| --- | --- | --- | --- | --- | --- | --- | --------- | ---- | ------- | ------- | ------- | ---------- | ------------- | --- |
Adria` Puigdome`nech Badia, Pablo Sprechmann, Alex Vitvitskyi, In International conference on machine learning, pages 49–58.
| Daniel  | Guo, Bilal                             | Piot,     | Steven | Kapturowski,   | Olivier       | Tieleman, | PMLR,2016.                         |     |         |               |     |                       |       |          |
| ------- | -------------------------------------- | --------- | ------ | -------------- | ------------- | --------- | ---------------------------------- | --- | ------- | ------------- | --- | --------------------- | ----- | -------- |
| Mart´ın | Arjovsky,                              | Alexander |        | Pritzel, Andew | Bolt, et      | al. Never |                                    |     |         |               |     |                       |       |          |
|         |                                        |           |        |                |               |           | JustinFu,KatieLuo,andSergeyLevine. |     |         |               |     | Learningrobustrewards |       |          |
| giveup: | Learningdirectedexplorationstrategies. |           |        |                | arXivpreprint |           |                                    |     |         |               |     |                       |       |          |
|         |                                        |           |        |                |               |           | with adversarial                   |     | inverse | reinforcement |     | learning.             | arXiv | preprint |
arXiv:2002.06038,2020.
arXiv:1710.11248,2017.
| Yuntao Bai, | Saurav | Kadavath, | Sandipan | Kundu, | Amanda | Askell, |              |         |         |       |       |        |          |     |
| ----------- | ------ | --------- | -------- | ------ | ------ | ------- | ------------ | ------- | ------- | ----- | ----- | ------ | -------- | --- |
|             |        |           |          |        |        |         | Adam Gleave, | Michael | Dennis, | Shane | Legg, | Stuart | Russell, | and |
JacksonKernion,AndyJones,AnnaChen,AnnaGoldie,Azalia
|                                  |     |     |     |                   |     |       | Jan Leike. | Quantifying |     | differences | in  | reward | functions. | arXiv |
| -------------------------------- | --- | --- | --- | ----------------- | --- | ----- | ---------- | ----------- | --- | ----------- | --- | ------ | ---------- | ----- |
| Mirhoseini,CameronMcKinnon,etal. |     |     |     | Constitutionalai: |     | Harm- |            |             |     |             |     |        |            |       |
preprintarXiv:2006.13900,2020.
| lessness | from | ai feedback. |     | arXiv preprint | arXiv:2212.08073, |     |     |     |     |     |     |     |     |     |
| -------- | ---- | ------------ | --- | -------------- | ----------------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
2022. Paul W Glimcher. Understanding dopamine and reinforcement
|     |     |     |     |     |     |     | learning: | thedopaminerewardpredictionerrorhypothesis. |     |     |     |     |     | Pro- |
| --- | --- | --- | --- | --- | --- | --- | --------- | ------------------------------------------- | --- | --- | --- | --- | --- | ---- |
AndrewGBarto,SatinderSingh,NuttapongChentanez,etal.Intrin-
|                                                            |     |     |     |     |     |     | ceedings | of the | National | Academy | of  | Sciences, | 108:15647– |     |
| ---------------------------------------------------------- | --- | --- | --- | --- | --- | --- | -------- | ------ | -------- | ------- | --- | --------- | ---------- | --- |
| sicallymotivatedlearningofhierarchicalcollectionsofskills. |     |     |     |     |     | In  |          |        |          |         |     |           |            |     |
15654,2011.
Proceedingsofthe3rdInternationalConferenceonDevelopment
andLearning,volume112,page19.Citeseer,2004. Daya Guo, Dejian Yang, Haowei Zhang, Junxiao Song, Ruoyu
Zhang,RunxinXu,QihaoZhu,ShirongMa,PeiyiWang,XiaoBi,
KateBaumli,SatinderBaveja,FeryalBehbahani,HarrisChan,Ghe-
etal. Deepseek-r1:Incentivizingreasoningcapabilityinllmsvia
orgheComanici,SebastianFlennerhag,MaximeGazeau,Kristian
|     |     |     |     |     |     |     | reinforcementlearning. |     |     | arXivpreprintarXiv:2501.12948,2025. |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | ---------------------- | --- | --- | ----------------------------------- | --- | --- | --- | --- |
Holsheimer,DanHorgan,MichaelLaskin,etal.Vision-language
modelsasasourceofrewards.arXivpreprintarXiv:2312.09187, HarryFHarlow. Learningandsatiationofresponseinintrinsically
| 2023. |     |     |     |     |     |     | motivatedcomplexpuzzleperformancebymonkeys. |     |     |     |     |     |     | Journalof |
| ----- | --- | --- | --- | --- | --- | --- | ------------------------------------------- | --- | --- | --- | --- | --- | --- | --------- |
comparativeandphysiologicalpsychology,43(4):289,1950.
MarcBellemare,SriramSrinivasan,GeorgOstrovski,TomSchaul,
David Saxton, and Remi Munos. Unifying count-based explo- KristianHartikainen,XinyangGeng,TuomasHaarnoja,andSergey
rationandintrinsicmotivation. Advancesinneuralinformation Levine. Dynamical distance learning for semi-supervised and
processingsystems,29,2016. unsupervisedskilldiscovery. arXivpreprintarXiv:1907.08225,
2019.
| Ralph Allan | Bradley | and | Milton | E Terry. | Rank analysis | of in- |     |     |     |     |     |     |     |     |
| ----------- | ------- | --- | ------ | -------- | ------------- | ------ | --- | --- | --- | --- | --- | --- | --- | --- |
complete block designs: I. the method of paired comparisons. Rein Houthooft, Xi Chen, Yan Duan, John Schulman, Filip
Biometrika,39(3/4):324–345,1952. DeTurck,andPieterAbbeel.Vime:Variationalinformationmax-
Yuri Burda, Harrison Edwards, Amos Storkey, and Oleg Klimov. imizingexploration. Advancesinneuralinformationprocessing
| Exploration | by  | random | network | distillation. | arXiv | preprint | systems,29,2016. |     |     |     |     |     |     |     |
| ----------- | --- | ------ | ------- | ------------- | ----- | -------- | ---------------- | --- | --- | --- | --- | --- | --- | --- |
arXiv:1810.12894,2018. ErikJennerandAdamGleave. Preprocessingrewardfunctionsfor
|                    |     |            |     |        |                |       | interpretability. |     | arXivpreprintarXiv:2203.13553,2022. |     |     |     |     |     |
| ------------------ | --- | ---------- | --- | ------ | -------------- | ----- | ----------------- | --- | ----------------------------------- | --- | --- | --- | --- | --- |
| Paul F Christiano, |     | Jan Leike, | Tom | Brown, | Miljan Martic, | Shane |                   |     |                                     |     |     |     |     |     |
Legg,andDarioAmodei. Deepreinforcementlearningfromhu- Wonseok Jeon, Chen-Yang Su, Paul Barde, Thang Doan, Derek
manpreferences.Advancesinneuralinformationprocessingsys- Nowrouzezahrai, and Joelle Pineau. Regularized inverse rein-
tems,30,2017. forcementlearning. arXivpreprintarXiv:2010.03691,2020.
JosefDai,XuehaiPan,RuiyangSun,JiamingJi,XinboXu,Mickel TimoKaufmann,PaulWeng,ViktorBengs,andEykeHu¨llermeier.
Liu, Yizhou Wang, and Yaodong Yang. Safe rlhf: Safe re- Asurveyofreinforcementlearningfromhumanfeedback. arXiv
inforcement learning from human feedback. arXiv preprint preprintarXiv:2312.14925,10,2023.
arXiv:2310.12773,2023.
|     |     |     |     |     |     |     | Jin-Gyeom | Kim and | Bowon | Lee. | Automatic | p2p | energy | trading |
| --- | --- | --- | --- | --- | --- | --- | --------- | ------- | ----- | ---- | --------- | --- | ------ | ------- |
YuqingDu,OliviaWatkins,ZihanWang,Ce´dricColas,TrevorDar- modelbasedonreinforcementlearningusinglongshort-termde-
rell,PieterAbbeel,AbhishekGupta,andJacobAndreas.Guiding layedreward. Energies,13(20):5359,2020.

Changyeon Kim, Jongjin Park, Jinwoo Shin, Honglak Lee, Pieter RussellMendonca,OlehRybkin,KostasDaniilidis,DanijarHafner,
Abbeel, and Kimin Lee. Preference transformer: Modeling andDeepakPathak. Discoveringandachievinggoalsviaworld
human preferences using transformers for rl. arXiv preprint models. Advances in Neural Information Processing Systems,
| arXiv:2303.00957,2023. |     |     |     |     |     |     | 34:24379–24391,2021. |     |     |     |     |     |     |
| ---------------------- | --- | --- | --- | --- | --- | --- | -------------------- | --- | --- | --- | --- | --- | --- |
Martin Klissarov, Pierluca D’Oro, Shagun Sodhani, Roberta VivekMyers,EvanEllis,SergeyLevine,BenjaminEysenbach,and
Raileanu, Pierre-Luc Bacon, Pascal Vincent, Amy Zhang, and Anca Dragan. Learning to assist humans without inferring re-
MikaelHenaff. Motif:Intrinsicmotivationfromartificialintelli- wards. arXivpreprintarXiv:2411.02623,2024.
| gencefeedback. |     | arXivpreprintarXiv:2310.00166,2023. |     |     |     |     |     |     |     |     |     |     |     |
| -------------- | --- | ----------------------------------- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
OfirNachum,ShixiangShaneGu,HonglakLee,andSergeyLevine.
Alexander S Klyubin, Daniel Polani, and Chrystopher L Nehaniv. Data-efficient hierarchical reinforcement learning. Advances in
Empowerment: Auniversalagent-centricmeasureofcontrol. In neuralinformationprocessingsystems,31,2018.
2005ieeecongressonevolutionarycomputation,volume1,pages GeorgOstrovski,MarcGBellemare,Aa¨ronOord,andRe´miMunos.
128–135.IEEE,2005. Count-based exploration with neural density models. In In-
|           |       |          |                 |     |        |              | ternational | conference | on  | machine learning, |     | pages | 2721–2730. |
| --------- | ----- | -------- | --------------- | --- | ------ | ------------ | ----------- | ---------- | --- | ----------------- | --- | ----- | ---------- |
| W Bradley | Knox, | Stephane | Hatgis-Kessell, |     | Serena | Booth, Scott |             |            |     |                   |     |       |            |
PMLR,2017.
| Niekum, | Peter Stone, | and | Alessandro | Allievi. |     | Models of hu- |     |     |     |     |     |     |     |
| ------- | ------------ | --- | ---------- | -------- | --- | ------------- | --- | --- | --- | --- | --- | --- | --- |
man preference for learning reward functions. arXiv preprint Soraia Oueida, Moayad Aloqaily, and Sorin Ionescu. A smart
arXiv:2206.02231,2022. healthcare reward model for resource allocation in smart city.
Multimediatoolsandapplications,78:24573–24594,2019.
| W Bradley | Knox, | Alessandro | Allievi, | Holger | Banzhaf, | Felix |     |     |     |     |     |     |     |
| --------- | ----- | ---------- | -------- | ------ | -------- | ----- | --- | --- | --- | --- | --- | --- | --- |
Schmitt,andPeterStone. Reward(mis)designforautonomous LongOuyang,JeffreyWu,XuJiang,DiogoAlmeida,CarrollWain-
driving. ArtificialIntelligence,316:103829,2023. wright,PamelaMishkin,ChongZhang,SandhiniAgarwal,Kata-
|                |         |          |                |          |         |               | rinaSlama,AlexRay,etal.        |     |     | Traininglanguagemodelstofollow |     |     |     |
| -------------- | ------- | -------- | -------------- | -------- | ------- | ------------- | ------------------------------ | --- | --- | ------------------------------ | --- | --- | --- |
| Andreas Ko¨pf, | Yannic  | Kilcher, | Dimitri        | Von      | Ru¨tte, | Sotiris Anag- |                                |     |     |                                |     |     |     |
|                |         |          |                |          |         |               | instructionswithhumanfeedback. |     |     | Advancesinneuralinforma-       |     |     |     |
| nostidis,      | Zhi Rui | Tam,     | Keith Stevens, | Abdullah |         | Barhoum, Duc  |                                |     |     |                                |     |     |     |
tionprocessingsystems,35:27730–27744,2022.
| Nguyen,                     | Oliver | Stanley, | Richa´rd | Nagyfi,  | et al. | Openassistant |              |             |              |                  |     |        |          |
| --------------------------- | ------ | -------- | -------- | -------- | ------ | ------------- | ------------ | ----------- | ------------ | ---------------- | --- | ------ | -------- |
|                             |        |          |          |          |        |               | SeohongPark, | OlehRybkin, |              | andSergeyLevine. |     | Metra: | Scalable |
| conversations-democratizing |        |          | large    | language | model  | alignment.    |              |             |              |                  |     |        |          |
|                             |        |          |          |          |        |               | unsupervised | rl with     | metric-aware | abstraction.     |     | arXiv  | preprint |
AdvancesinNeuralInformationProcessingSystems,36:47669–
arXiv:2310.08887,2023.
47681,2023.
DeepakPathak,PulkitAgrawal,AlexeiAEfros,andTrevorDarrell.
TzeLeungLaiandHerbertRobbins.Asymptoticallyefficientadap-
Curiosity-drivenexplorationbyself-supervisedprediction.InIn-
| tiveallocationrules. |     | Advancesinappliedmathematics, |     |     |     | 6(1):4– |             |            |     |                   |     |       |            |
| -------------------- | --- | ----------------------------- | --- | --- | --- | ------- | ----------- | ---------- | --- | ----------------- | --- | ----- | ---------- |
|                      |     |                               |     |     |     |         | ternational | conference | on  | machine learning, |     | pages | 2778–2787. |
22,1985.
PMLR,2017.
HarrisonLee,SamratPhatale,HassanMansoor,ThomasMesnard,
DeepakPathak,DhirajGandhi,andAbhinavGupta.Self-supervised
JohanFerret, KellieRenLu, ColtonBishop, EthanHall, Victor explorationviadisagreement.InInternationalconferenceonma-
Carbune,AbhinavRastogi,etal.Rlaifvs.rlhf:Scalingreinforce- chinelearning,pages5062–5071.PMLR,2019.
| mentlearningfromhumanfeedbackwithaifeedback. |     |     |     |     |     | InInter- |                   |     |          |                  |     |         |        |
| -------------------------------------------- | --- | --- | --- | --- | --- | -------- | ----------------- | --- | -------- | ---------------- | --- | ------- | ------ |
|                                              |     |     |     |     |     |          | Robin L Plackett. | The | analysis | of permutations. |     | Journal | of the |
nationalConferenceonMachineLearning,pages26874–26901.
RoyalStatisticalSocietySeriesC:AppliedStatistics,24(2):193–
PMLR,2024.
202,1975.
| HaoLiuandPieterAbbeel. |     |     | Behaviorfromthevoid: |     |     | Unsupervised |     |     |     |     |     |     |     |
| ---------------------- | --- | --- | -------------------- | --- | --- | ------------ | --- | --- | --- | --- | --- | --- | --- |
RafaelRafailov,ArchitSharma,EricMitchell,ChristopherDMan-
| activepre-training. |     | AdvancesinNeuralInformationProcessing |     |     |     |     |               |        |     |               |        |            |     |
| ------------------- | --- | ------------------------------------- | --- | --- | --- | --- | ------------- | ------ | --- | ------------- | ------ | ---------- | --- |
|                     |     |                                       |     |     |     |     | ning, Stefano | Ermon, | and | Chelsea Finn. | Direct | preference | op- |
Systems,34:18459–18473,2021.
|     |     |     |     |     |     |     | timization: | Your | language | model is | secretly | a reward | model. |
| --- | --- | --- | --- | --- | --- | --- | ----------- | ---- | -------- | -------- | -------- | -------- | ------ |
Minghuan Liu, Menghui Zhu, and Weinan Zhang. Goal- AdvancesinNeuralInformationProcessingSystems,36:53728–
| conditioned | reinforcement |     | learning: | Problems |     | and solutions. | 53741,2023. |     |     |     |     |     |     |
| ----------- | ------------- | --- | --------- | -------- | --- | -------------- | ----------- | --- | --- | --- | --- | --- | --- |
arXiv:2201.08299,2022. JuanRocamonde,VictorianoMontesinos,ElvisNava,EthanPerez,
|            |              |          |           |               |        |              | and David | Lindner.   | Vision-language |     | models    | are   | zero-shot |
| ---------- | ------------ | -------- | --------- | ------------- | ------ | ------------ | --------- | ---------- | --------------- | --- | --------- | ----- | --------- |
| Shang Liu, | Yu Pan,      | Guanting | Chen,     | and Xiaocheng |        | Li. Reward   |           |            |                 |     |           |       |           |
|            |              |          |           |               |        |              | reward    | models for | reinforcement   |     | learning. | arXiv | preprint  |
| modeling   | with ordinal |          | feedback: | Wisdom        | of the | crowd. arXiv |           |            |                 |     |           |       |           |
arXiv:2310.12921,2023.
preprintarXiv:2411.12843,2024.
|             |           |           |             |     |             |             | Richard M  | Ryan and                            | Edward | L Deci. | Intrinsic | and extrinsic | mo- |
| ----------- | --------- | --------- | ----------- | --- | ----------- | ----------- | ---------- | ----------------------------------- | ------ | ------- | --------- | ------------- | --- |
| Tianqi Liu, | Zhen Qin, | Junru     | Wu, Jiaming |     | Shen, Misha | Khalman,    |            |                                     |        |         |           |               |     |
|             |           |           |             |     |             |             | tivations: | Classicdefinitionsandnewdirections. |        |         |           | Contemporary  |     |
| Rishabh     | Joshi,    | Yao Zhao, | Mohammad    |     | Saleh,      | Simon Baum- |            |                                     |        |         |           |               |     |
educationalpsychology,25(1):54–67,2000.
| gartner, | Jialu Liu, | et al. | Lipo: | Listwise | preference | optimiza- |     |     |     |     |     |     |     |
| -------- | ---------- | ------ | ----- | -------- | ---------- | --------- | --- | --- | --- | --- | --- | --- | --- |
tionthroughlearning-to-rank. arXivpreprintarXiv:2402.01878, Ramanan Sekar, Oleh Rybkin, Kostas Daniilidis, Pieter Abbeel,
2024. DanijarHafner,andDeepakPathak.Planningtoexploreviaself-
supervisedworldmodels.InInternationalconferenceonmachine
TrungQuocLuong,XinboZhang,ZhanmingJie,PengSun,Xiaoran
learning,pages8583–8592.PMLR,2020.
| Jin, andHangLi. |     | Reft: | Reasoningwithreinforcedfine-tuning. |     |     |     |     |     |     |     |     |     |     |
| --------------- | --- | ----- | ----------------------------------- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
DavidSilver,AjaHuang,ChrisJMaddison,ArthurGuez,Laurent
arXivpreprintarXiv:2401.08967,2024.
|     |     |     |     |     |     |     | Sifre, George | Van | Den Driessche, | Julian | Schrittwieser, |     | Ioannis |
| --- | --- | --- | --- | --- | --- | --- | ------------- | --- | -------------- | ------ | -------------- | --- | ------- |
Pietro Mazzaglia, Tim Verbelen, Bart Dhoedt, Alexandre Lacoste, Antonoglou,VedaPanneershelvam,MarcLanctot,etal. Master-
andSaiRajeswar. Choreographer: Learningandadaptingskills ing the game of go with deep neural networks and tree search.
inimagination. arXivpreprintarXiv:2211.13350,2022. nature,529(7587):484–489,2016.
PietroMazzaglia,TimVerbelen,BartDhoedt,AaronCourville,and Joar Skalse, Lucy Farnik, Sumeet Ramesh Motwani, Erik Jenner,
Sai Rajeswar. Genrl: Multimodal-foundation world models for Adam Gleave, and Alessandro Abate. Starc: A general frame-
generalizationinembodiedagents. Advancesinneuralinforma- workforquantifyingdifferencesbetweenrewardfunctions.arXiv
tionprocessingsystems,37:27529–27555,2024. preprintarXiv:2309.15257,2023.

Feifan Song, Bowen Yu, Minghao Li, Haiyang Yu, Fei Huang, Wanqi Xue, Qingpeng Cai, Zhenghai Xue, Shuo Sun, Shuchang
YongbinLi,andHoufengWang.Preferencerankingoptimization Liu, Dong Zheng, Peng Jiang, Kun Gai, and Bo An. Prefrec:
forhumanalignment. InProceedingsoftheAAAIConferenceon Recommender systems with human preferences for reinforcing
ArtificialIntelligence,volume38,pages18990–18998,2024. long-term user engagement. In Proceedings of the 29th ACM
SIGKDDConferenceonKnowledgeDiscoveryandDataMining,
| Sumedh | Sontakke, | Jesse Zhang, | Se´b | Arnold, | Karl | Pertsch, | Erdem |     |     |     |     |
| ------ | --------- | ------------ | ---- | ------- | ---- | -------- | ----- | --- | --- | --- | --- |
pages2874–2884,2023.
| Bıyık, | DorsaSadigh, | ChelseaFinn, |     | andLaurentItti. |     |     | Roboclip: |     |     |     |     |
| ------ | ------------ | ------------ | --- | --------------- | --- | --- | --------- | --- | --- | --- | --- |
Onedemonstrationisenoughtolearnrobotpolicies.Advancesin Lunjun Zhang, Arian Hosseini, Hritik Bansal, Mehran Kazemi,
NeuralInformationProcessingSystems,36:55681–55693,2023. Aviral Kumar, and Rishabh Agarwal. Generative verifiers:
|                                   |     |     |     |     |                        |     |     | Reward modeling | as next-token | prediction. | arXiv preprint |
| --------------------------------- | --- | --- | --- | --- | ---------------------- | --- | --- | --------------- | ------------- | ----------- | -------------- |
| RichardSSutton,AndrewGBarto,etal. |     |     |     |     | Reinforcementlearning: |     |     |                 |               |             |                |
arXiv:2408.15240,2024.
| Anintroduction,volume1. |     |     | MITpressCambridge,1998. |     |     |     |     |                  |              |                   |             |
| ----------------------- | --- | --- | ----------------------- | --- | --- | --- | --- | ---------------- | ------------ | ----------------- | ----------- |
|                         |     |     |                         |     |     |     |     | Brian D Ziebart, | Andrew Maas, | J Andrew Bagnell, | and Anind K |
HaoranTang,ReinHouthooft,DavisFoote,AdamStooke,OpenAI Dey. Maximum entropy inverse reinforcement learning. In
XiChen, YanDuan, JohnSchulman, FilipDeTurck, andPieter Proceedings of the 23rd national conference on Artificial
Abbeel. # exploration: A study of count-based exploration for intelligence-Volume3,pages1433–1438,2008.
|                    |     |           | Advances |     | in neural | information |     |     |     |     |     |
| ------------------ | --- | --------- | -------- | --- | --------- | ----------- | --- | --- | --- | --- | --- |
| deep reinforcement |     | learning. |          |     |           |             |     |     |     |     |     |
processingsystems,30,2017.
| Hugo Touvron,  |            | Louis Martin,  | Kevin   | Stone,       | Peter      | Albert, | Am-      |     |     |     |     |
| -------------- | ---------- | -------------- | ------- | ------------ | ---------- | ------- | -------- | --- | --- | --- | --- |
| jad Almahairi, |            | Yasmine        | Babaei, | Nikolay      | Bashlykov, |         | Soumya   |     |     |     |     |
| Batra,         | Prajjwal   | Bhargava,      | Shruti  | Bhosale,     | et         | al.     | Llama 2: |     |     |     |     |
| Open           | foundation | and fine-tuned |         | chat models. |            | arXiv   | preprint |     |     |     |     |
arXiv:2307.09288,2023.
| MarkTowers,ArielKwiatkowski,JordanTerry,etal. |           |     |               |     |          | Gymnasium:    |     |     |     |     |     |
| --------------------------------------------- | --------- | --- | ------------- | --- | -------- | ------------- | --- | --- | --- | --- | --- |
| A standard                                    | interface | for | reinforcement |     | learning | environments. |     |     |     |     |     |
arXiv:2407.17032,2024.
| Mudit Verma | and      | Katherine | Metcalf. |              | Hindsight | priors | for      |     |     |     |     |
| ----------- | -------- | --------- | -------- | ------------ | --------- | ------ | -------- | --- | --- | --- | --- |
| reward      | learning | from      | human    | preferences. |           | arXiv  | preprint |     |     |     |     |
arXiv:2404.08828,2024.
| Shenghua | Wan,     | Hai-Hang | Sun, Le | Gan,              | and De-Chuan |           | Zhan. |     |     |     |     |
| -------- | -------- | -------- | ------- | ----------------- | ------------ | --------- | ----- | --- | --- | --- | --- |
| Moser:   | learning | sensory  | policy  | for task-specific |              | viewpoint | via   |     |     |     |     |
view-conditionalworldmodel.InProceedingsoftheThirty-Third
| International |     | Joint Conference | on  | Artificial | Intelligence, |     | pages |     |     |     |     |
| ------------- | --- | ---------------- | --- | ---------- | ------------- | --- | ----- | --- | --- | --- | --- |
5046–5054,2024.
| Yufei Wang, | Zhanyi | Sun, Jesse      | Zhang,        | Zhou      | Xian,      | Erdem         | Biyik,  |     |     |     |     |
| ----------- | ------ | --------------- | ------------- | --------- | ---------- | ------------- | ------- | --- | --- | --- | --- |
| David       | Held,  | and Zackory     | Erickson.     | Rl-vlm-f: |            | reinforcement |         |     |     |     |     |
| learning    | from   | vision language | foundation    |           | model      | feedback.     | In      |     |     |     |     |
| Proceedings |        | of the 41st     | International |           | Conference | on            | Machine |     |     |     |     |
Learning,pages51484–51501,2024.
YucenWang,RuiYu,ShenghuaWan,LeGan,andDe-ChuanZhan.
| Founder:   | Grounding | foundation |         | models | in world         | models | for  |     |     |     |     |
| ---------- | --------- | ---------- | ------- | ------ | ---------------- | ------ | ---- | --- | --- | --- | --- |
| open-ended | embodied  | decision   | making. |        | In International |        | Con- |     |     |     |     |
ferenceonMachineLearning,2025.
BlakeWulfe,AshwinBalakrishna,LoganEllis,JeanMercat,Rowan
| McAllister, | and    | Adrien     | Gaidon. | Dynamics-aware |                   | comparison |     |     |     |     |     |
| ----------- | ------ | ---------- | ------- | -------------- | ----------------- | ---------- | --- | --- | --- | --- | --- |
| of learned  | reward | functions. | arXiv   | preprint       | arXiv:2201.10081, |            |     |     |     |     |     |
2022.
TianbaoXie,SihengZhao,ChenHenryWu,YitaoLiu,QianLuo,
| VictorZhong, |     | YanchaoYang, | andTaoYu. |     | Text2reward: |     | Auto- |     |     |     |     |
| ------------ | --- | ------------ | --------- | --- | ------------ | --- | ----- | --- | --- | --- | --- |
mateddenserewardfunctiongenerationforreinforcementlearn-
ing. arXivpreprintarXiv:2309.11489,2023.
| HuajianXin, | ZZRen, | JunxiaoSong, |       | ZhihongShao, |      | WanjiaZhao, |     |     |     |     |     |
| ----------- | ------ | ------------ | ----- | ------------ | ---- | ----------- | --- | --- | --- | --- | --- |
| Haocheng    | Wang,  | Bo Liu,      | Liyue | Zhang,       | Xuan | Lu, Qiushi  | Du, |     |     |     |     |
etal.Deepseek-prover-v1.5:Harnessingproofassistantfeedback
| for reinforcement |     | learning | and monte-carlo |     | tree | search. | arXiv |     |     |     |     |
| ----------------- | --- | -------- | --------------- | --- | ---- | ------- | ----- | --- | --- | --- | --- |
preprintarXiv:2408.08152,2024.
| Jiacheng   | Xu, Chao      | Chen,      | Fuxiang      | Zhang,         | Lei Yuan, | Zongzhang |            |     |     |     |     |
| ---------- | ------------- | ---------- | ------------ | -------------- | --------- | --------- | ---------- | --- | --- | --- | --- |
| Zhang,     | and           | Yang Yu.   | Internal     | logical        | induction |           | for pixel- |     |     |     |     |
| symbolic   | reinforcement |            | learning.    | In Proceedings |           | of        | the 29th   |     |     |     |     |
| ACM SIGKDD |               | Conference | on Knowledge |                | Discovery |           | and Data   |     |     |     |     |
Mining,pages2825–2837,2023.

## Extracted Images

### Page 2

![page002_img001.png](img/page002_img001.png)
![page002_img002.png](img/page002_img002.png)
![page002_img003.png](img/page002_img003.png)
![page002_img004.png](img/page002_img004.png)
![page002_img005.png](img/page002_img005.png)
![page002_img006.png](img/page002_img006.png)
![page002_img007.png](img/page002_img007.png)
![page002_img008.png](img/page002_img008.png)
![page002_img009.png](img/page002_img009.png)
