| SEARCHING |     |     | ACTIVATION |     |     | FUNCTIONS |     |     |     |     |
| --------- | --- | --- | ---------- | --- | --- | --------- | --- | --- | --- | --- |
FOR
PrajitRamachandran∗,BarretZoph,QuocV.Le
GoogleBrain
{prajit,barretzoph,qvl}@google.com
ABSTRACT
|     | The choice | of activation |     | functions | in deep | networks | has | a significant | effect | on  |
| --- | ---------- | ------------- | --- | --------- | ------- | -------- | --- | ------------- | ------ | --- |
7102 tcO 72  ]EN.sc[  2v14950.0171:viXra the training dynamics and task performance. Currently, the most successful and
|     | widely-used | activation | function | is  | the Rectified |     | Linear | Unit (ReLU). | Although |     |
| --- | ----------- | ---------- | -------- | --- | ------------- | --- | ------ | ------------ | -------- | --- |
varioushand-designedalternativestoReLUhavebeenproposed,nonehaveman-
|     | aged to        | replace it                                                 | due to     | inconsistent | gains.        | In  | this work,     | we propose        | to  | lever-  |
| --- | -------------- | ---------------------------------------------------------- | ---------- | ------------ | ------------- | --- | -------------- | ----------------- | --- | ------- |
|     | age automatic  | search                                                     | techniques |              | to discover   | new | activation     | functions.        |     | Using   |
|     | a combination  | of                                                         | exhaustive | and          | reinforcement |     | learning-based | search,           | we  | dis-    |
|     | cover multiple | novel                                                      | activation | functions.   |               | We  | verify         | the effectiveness |     | of the  |
|     | searches       | by conducting                                              | an         | empirical    | evaluation    |     | with the       | best discovered   |     | activa- |
|     | tionfunction.  | Ourexperimentsshowthatthebestdiscoveredactivationfunction, |            |              |               |     |                |                   |     |         |
f(x)=x·sigmoid(βx),whichwenameSwish,tendstoworkbetterthanReLU
|     | ondeepermodelsacrossanumberofchallengingdatasets. |     |     |     |     |     |     | Forexample, |     | simply |
| --- | ------------------------------------------------- | --- | --- | --- | --- | --- | --- | ----------- | --- | ------ |
replacingReLUswithSwishunitsimprovestop-1classificationaccuracyonIm-
|     | ageNet     | by 0.9% for | Mobile  | NASNet-A   | and     | 0.6% | for Inception-ResNet-v2. |                        |     | The |
| --- | ---------- | ----------- | ------- | ---------- | ------- | ---- | ------------------------ | ---------------------- | --- | --- |
|     | simplicity | of Swish    | and its | similarity | to ReLU | make | it                       | easy for practitioners |     | to  |
replaceReLUswithSwishunitsinanyneuralnetwork.
1 INTRODUCTION
At the heart of every deep network lies a linear transformation followed by an activation func-
tion f(·). The activation function plays a major role in the success of training deep neural net-
works. Currently, the most successful and widely-used activation function is the Rectified Lin-
ear Unit (ReLU) (Hahnloser et al., 2000; Jarrett et al., 2009; Nair & Hinton, 2010), defined as
f(x)=max(x,0). TheuseofReLUswasabreakthroughthatenabledthefullysupervisedtraining
of state-of-the-art deep networks (Krizhevsky et al., 2012). Deep networks with ReLUs are more
easilyoptimizedthannetworkswithsigmoidortanhunits,becausegradientsareabletoflowwhen
the input to the ReLU function is positive. Thanks to its simplicity and effectiveness, ReLU has
becomethedefaultactivationfunctionusedacrossthedeeplearningcommunity.
While numerous activation functions have been proposed to replace ReLU (Maas et al., 2013; He
etal.,2015;Clevertetal.,2015;Klambaueretal.,2017),nonehavemanagedtogainthewidespread
adoptionthatReLUenjoys. ManypractitionershavefavoredthesimplicityandreliabilityofReLU
because the performance improvements of the other activation functions tend to be inconsistent
acrossdifferentmodelsanddatasets.
The activation functions proposed to replace ReLU were hand-designed to fit properties deemed
to be important. However, the use of search techniques to automate the discovery of traditionally
human-designedcomponentshasrecentlyshowntobeextremelyeffective(Zoph&Le,2016;Bello
et al., 2017; Zoph et al., 2017). For example, Zoph et al. (2017) used reinforcement learning-
basedsearchtofindareplicableconvolutionalcellthatoutperformshuman-designedarchitectures
onImageNet.
Inthiswork,weuseautomatedsearchtechniquestodiscovernovelactivationfunctions. Wefocus
onfindingnewscalaractivationfunctions,whichtakeinasinputascalarandoutputascalar,because
scalaractivationfunctionscanbeusedtoreplacetheReLUfunctionwithoutchangingthenetwork
architecture. Usingacombinationofexhaustiveandreinforcementlearning-basedsearch, wefind
a number of novel activation functions that show promising performance. To further validate the
∗WorkdoneasamemberoftheGoogleBrainResidencyprogram(g.co/brainresidency).
1

effectiveness of using searches to discover scalar activation functions, we empirically evaluate the
bestdiscoveredactivationfunction. Thebestdiscoveredactivationfunction,whichwecallSwish,is
f(x) = x·sigmoid(βx),whereβ isaconstantortrainableparameter. Ourextensiveexperiments
showthatSwishconsistentlymatchesoroutperformsReLUondeepnetworksappliedtoavariety
ofchallengingdomainssuchasimageclassificationandmachinetranslation. OnImageNet,replac-
ingReLUswithSwishunitsimprovestop-1classificationaccuracyby0.9%onMobileNASNet-A
(Zophetal.,2017)and0.6%onInception-ResNet-v2(Szegedyetal.,2017). Theseaccuracygains
aresignificantgiventhatoneyearofarchitecturaltuningandenlargingyielded1.3%accuracyim-
provementgoingfromInceptionV3(Szegedyetal.,2016)toInception-ResNet-v2(Szegedyetal.,
2017).
2 METHODS
In order to utilize search techniques, a search space that contains promising candidate activation
functions must be designed. An important challenge in designing search spaces is balancing the
sizeandexpressivityofthesearchspace. Anoverlyconstrainedsearchspacewillnotcontainnovel
activationfunctions, whereasasearchspacethatistoolargewillbedifficulttoeffectivelysearch.
Tobalancethetwocriteria,wedesignasimplesearchspaceinspiredbytheoptimizersearchspace
ofBelloetal.(2017)thatcomposesunaryandbinaryfunctionstoconstructtheactivationfunction.
x Unary Core unit
Binary Unary
x Unary
Binary
x Unary
Figure1:Anexampleactivationfunctionstructure. Theactivationfunctioniscomposedofmultiple
repetitions of the “core unit”, which consists of two inputs, two unary functions, and one binary
function. Unaryfunctionstakeinasinglescalarinputandreturnasinglescalaroutput,suchu(x)=
x2oru(x)=σ(x).Binaryfunctionstakeintwoscalarinputsandreturnasinglescalaroutput,such
asb(x ,x )=x ·x orb(x ,x )=exp(−(x −x )2).
1 2 1 2 1 2 1 2
AsshowninFigure1,theactivationfunctionisconstructedbyrepeatedlycomposingthethe“core
unit”,whichisdefinedasb(u (x ),u (x )). Thecoreunittakesintwoscalarinputs,passeseach
1 1 2 2
inputindependentlythroughanunaryfunction, andcombinesthetwounaryoutputswithabinary
functionthatoutputsascalar. Sinceouraimistofindscalaractivationfunctionswhichtransforma
singlescalarinputintoasinglescalaroutput,theinputsoftheunaryfunctionsarerestrictedtothe
layerpreactivationxandthebinaryfunctionoutputs.
Giventhesearchspace,thegoalofthesearchalgorithmistofindeffectivechoicesfortheunaryand
binaryfunctions. Thechoiceofthesearchalgorithmdependsonthesizeofthesearchspace. Ifthe
searchspaceissmall,suchaswhenusingasinglecoreunit,itispossibletoexhaustivelyenumerate
theentiresearchspace.Ifthecoreunitisrepeatedmultipletimes,thesearchspacewillbeextremely
large(i.e.,ontheorderof1012possibilities),makingexhaustivesearchinfeasible.
Forlargesearchspaces,weuseanRNNcontroller(Zoph&Le,2016),whichisvisualizedinFigure
2. At each timestep, the controller predicts a single component of the activation function. The
predictionisfedbacktothecontrollerinthenexttimestep,andthisprocessisrepeateduntilevery
componentoftheactivationfunctionispredicted. Thepredictedstringisthenusedtoconstructthe
activationfunction.
Once a candidate activation function has been generated by the search algorithm, a “child net-
work” with the candidate activation function is trained on some task, such as image classification
on CIFAR-10. After training, the validation accuracy of the child network is recorded and used
2

Binary Input 1 Input 2 Unary 1 Unary 2 Binary Input 1
... ...
Core unit N-1 Core unit N Core unit N+1
Figure 2: The RNN controller used to search over large spaces. At each step, it predicts a single
component of the activation function. The prediction is fed back as input to the next timestep in
anautoregressivefashion. Thecontrollerkeepspredictinguntileverycomponentoftheactivation
functionhasbeenchosen. Thecontrolleristrainedwithreinforcementlearning.
to update the search algorithm. In the case of exhaustive search, a list of the top performing acti-
vation functions ordered by validation accuracy is maintained. In the case of the RNN controller,
thecontrolleristrainedwithreinforcementlearningtomaximizethevalidationaccuracy,wherethe
validationaccuracyservesasthereward. Thistrainingpushesthecontrollertogenerateactivation
functionsthathavehighvalidationaccuracies.
Sinceevaluatingasingleactivationfunctionrequirestrainingachildnetwork,thesearchiscompu-
tationally expensive. To decrease the wall clock time required to conduct the search, a distributed
trainingschemeisusedtoparallelizethetrainingofeachchildnetwork. Inthisscheme,thesearch
algorithm proposes a batch of candidate activation functions which are added to a queue. Worker
machinespullactivationfunctionsoffthequeue,trainachildnetwork,andreportbackthefinalval-
idationaccuracyofthecorrespondingactivationfunction. Thevalidationaccuraciesareaggregated
andusedtoupdatethesearchalgorithm.
3 SEARCH FINDINGS
WeconductalloursearcheswiththeResNet-20(Heetal.,2016a)asthechildnetworkarchitecture,
and train on CIFAR-10 (Krizhevsky & Hinton, 2009) for 10K steps. This constrained environ-
mentcouldpotentiallyskewtheresultsbecausethetopperformingactivationfunctionsmightonly
perform well for small networks. However, we show in the experiments section that many of the
discoveredfunctionsgeneralizetolargermodels. Exhaustivesearchisusedforsmallsearchspaces,
whileanRNNcontrollerisusedforlargersearchspaces. TheRNNcontrolleristrainedwithPolicy
ProximalOptimization(Schulmanetal.,2017),usingtheexponentialmovingaverageofrewardsas
abaselinetoreducevariance. Thefulllistunaryandbinaryfunctionsconsideredareasfollows:
√
• Unaryfunctions: x,−x,|x|,x2,x3, x,βx,x+β,log(|x|+(cid:15)),exp(x)sin(x),cos(x),
sinh(x), cosh(x), tanh(x), sinh−1(x), tan−1(x), sinc(x), max(x,0), min(x,0), σ(x),
log(1+exp(x)),exp(−x2),erf(x),β
• Binaryfunctions: x 1 +x 2 ,x 1 ·x 2 ,x 1 −x 2 , x2 x + 1 (cid:15) ,max(x 1 ,x 2 ),min(x 1 ,x 2 ),σ(x 1 )·x 2 ,
exp(−β(x −x )2),exp(−β|x −x |),βx +(1−β)x
1 2 1 2 1 2
whereβ indicatesaper-channeltrainableparameterandσ(x) = (1+exp(−x))−1 isthesigmoid
function. Differentsearchspacesarecreatedbyvaryingthenumberofcoreunitsusedtoconstruct
theactivationfunctionandvaryingtheunaryandbinaryfunctionsavailabletothesearchalgorithm.
Figure 3 plots the top performing novel activation functions found by the searches. We highlight
severalnoteworthytrendsuncoveredbythesearches:
3

4 x · σ(βx)
x (sinh−1(x))2
·
min(x,sin(x))
2
| (tan−1(x))2 | x   |     |     |     | max(x,σ(x)) |     |
| ----------- | --- | --- | --- | --- | ----------- | --- |
−
|     |     |     |     |     | cos(x) | x   |
| --- | --- | --- | --- | --- | ------ | --- |
| 0   |     |     |     |     | −      |     |
max(x,tanh(x))
sinc(x)+x
2
4
| 6 4 | 2   | 0 2 4 | 6 6 4 | 2 0 | 2 4 | 6   |
| --- | --- | ----- | ----- | --- | --- | --- |
Figure3: Thetopnovelactivationfunctionsfoundbythesearches. Separatedintotwodiagramsfor
| visualclarity. | Bestviewedincolor. |     |     |     |     |     |
| -------------- | ------------------ | --- | --- | --- | --- | --- |
• Complicated activation functions consistently underperform simpler activation functions,
potentially due to an increased difficulty in optimization. The best performing activation
functionscanberepresentedby1or2coreunits.
•
Acommonstructuresharedbythetopactivationfunctionsistheuseoftherawpreactiva-
tionxasinputtothefinalbinaryfunction:b(x,g(x)).TheReLUfunctionalsofollowsthis
| structure,whereb(x |     | ,x )=max(x | ,x )andg(x)=0. |     |     |     |
| ------------------ | --- | ---------- | -------------- | --- | --- | --- |
|                    |     | 1 2        | 1 2            |     |     |     |
• Thesearchesdiscoveredactivationfunctionsthatutilizeperiodicfunctions,suchassinand
cos. The most common use of periodic functions is through addition or subtraction with
therawpreactivationx(oralinearlyscaledx). Theuseofperiodicfunctionsinactivation
functionshasonlybeenbrieflyexploredinpriorwork(Parascandoloetal.,2016),sothese
discoveredfunctionssuggestafruitfulrouteforfurtherresearch.
• Functions that use division tend to perform poorly because the output explodes when the
denominatorisnear0.
Divisionissuccessfulonlywhenfunctionsinthedenominatorare
eitherboundedawayfrom0,suchascosh(x),orapproach0onlywhenthenumeratoralso
approaches0,producinganoutputof1.
Sincetheactivationfunctionswerefoundusingarelativelysmallchildnetwork,theirperformance
maynotgeneralizewhenappliedtobiggermodels.Totesttherobustnessofthetopperformingnovel
activationfunctionstodifferentarchitectures,werunadditionalexperimentsusingthepreactivation
ResNet-164(RN)(Heetal.,2016b),WideResNet28-10(WRN)(Zagoruyko&Komodakis,2016),
andDenseNet100-12(DN)(Huangetal.,2017)models.Weimplementthe3modelsinTensorFlow
and replace the ReLU function with each of the top novel activation functions discovered by the
searches. Weusethesamehyperparametersdescribedineachwork,suchasoptimizingusingSGD
withmomentum,andfollowpreviousworksbyreportingthemedianof5differentruns.
| Function       | RN                | WRN DN    | Function       | RN                 | WRN DN    |     |
| -------------- | ----------------- | --------- | -------------- | ------------------ | --------- | --- |
| ReLU[max(x,0)] | 93.8              | 95.3 94.8 | ReLU[max(x,0)] | 74.2               | 77.8 83.7 |     |
| x·σ(βx)        | 94.5              | 95.5 94.9 | x·σ(βx)        | 75.1               | 78.0 83.9 |     |
| max(x,σ(x))    | 94.3              | 95.3 94.8 | max(x,σ(x))    | 74.8               | 78.6 84.2 |     |
| cos(x)−x       | 94.1              | 94.8 94.6 | cos(x)−x       | 75.2               | 76.6 81.8 |     |
| min(x,sin(x))  | 94.0              | 95.1 94.4 | min(x,sin(x))  | 73.4               | 77.1 74.3 |     |
| (tan−1(x))2−x  | 93.9              | 94.7 94.9 | (tan−1(x))2−x  | 75.2               | 76.7 83.1 |     |
| max(x,tanh(x)) | 93.9              | 94.2 94.5 | max(x,tanh(x)) | 74.8               | 76.0 78.6 |     |
| sinc(x)+x      | 91.5              | 92.1 92.0 | sinc(x)+x      | 66.1               | 68.3 67.9 |     |
| x·(sinh−1(x))2 | 85.1              | 92.1 91.1 | x·(sinh−1(x))2 | 52.8               | 70.6 68.1 |     |
| Table1:        | CIFAR-10accuracy. |           | Table2:        | CIFAR-100accuracy. |           |     |
The results are shown in Tables 1 and 2. Despite the changes in model architecture, six of the
eight activation functions successfully generalize. Of these six activation functions, all match or
outperformReLUonResNet-164.Furthermore,twoofthediscoveredactivationfunctions,x·σ(βx)
andmax(x,σ(x)),consistentlymatchoroutperformReLUonallthreemodels.
4

While these results are promising, it is still unclear whether the discovered activation functions
can successfully replace ReLU on challenging real world datasets. In order to validate the effec-
tiveness of the searches, in the rest of this work we focus on empirically evaluating the activation
function f(x) = x · σ(βx), which we call Swish. We choose to extensively evaluate Swish in-
stead of max(x,σ(x)) because early experimentation showed better generalization for Swish. In
the following sections, we analyze the properties of Swish and then conduct a thorough empirical
evaluationcomparingSwish,ReLU,andothercandidatebaselineactivationfunctionsonnumberof
largemodelsacrossavarietyoftasks.
4 SWISH
To recap, Swish is defined as x·σ(βx), where σ(z) = (1+exp(−z))−1 is the sigmoid function
andβ iseitheraconstantoratrainableparameter. Figure4plotsthegraphofSwishfordifferent
values of β. If β = 1, Swish is equivalent to the Sigmoid-weighted Linear Unit (SiL) of Elfwing
et al. (2017) that was proposed for reinforcement learning. If β = 0, Swish becomes the scaled
linear function f(x) = x. As β → ∞, the sigmoid component approaches a 0-1 function, so
2
SwishbecomesliketheReLUfunction.ThissuggeststhatSwishcanbelooselyviewedasasmooth
function which nonlinearly interpolates between the linear function and the ReLU function. The
degreeofinterpolationcanbecontrolledbythemodelifβ issetasatrainableparameter.
3 Swish 1.2 Swish first derivatives
β=0.1 β=0.1
β=1.0 β=1.0
β=10.0 1.0 β=10.0
2
0.8
1
0.6
0.4
0
0.2
1
0.0
2 0.2
5 4 3 2 1 0 1 2 3 6 4 2 0 2 4 6
Figure4: TheSwishactivationfunction. Figure5: FirstderivativesofSwish.
LikeReLU,Swishisunboundedaboveandboundedbelow.UnlikeReLU,Swishissmoothandnon-
monotonic. Infact,thenon-monotonicitypropertyofSwishdistinguishesitselffrommostcommon
activationfunctions. ThederivativeofSwishis
f(cid:48)(x)=σ(βx)+βx·σ(βx)(1−σ(βx))
=σ(βx)+βx·σ(βx)−βx·σ(βx)2
=βx·σ(x)+σ(βx)(1−βx·σ(βx))
=βf(x)+σ(βx)(1−βf(x))
ThefirstderivativeofSwishisshowninFigure5fordifferentvaluesofβ. Thescaleofβ controls
howfastthefirstderivativeasymptotesto0and1. Whenβ = 1,thederivativehasmagnitudeless
than1forinputsthatarelessthanaround1.25. Thus,thesuccessofSwishwithβ =1impliesthat
thegradientpreservingpropertyofReLU(i.e.,havingaderivativeof1whenx>0)maynolonger
beadistinctadvantageinmodernarchitectures.
ThemoststrikingdifferencebetweenSwishandReLUisthenon-monotonic“bump”ofSwishwhen
x<0.AsshowninFigure6,alargepercentageofpreactivationsfallinsidethedomainofthebump
(−5≤x≤0),whichindicatesthatthenon-monotonicbumpisanimportantaspectofSwish. The
shapeofthebumpcanbecontrolledbychangingtheβ parameter. Whilefixingβ = 1iseffective
inpractice,theexperimentssectionshowsthattrainingβcanfurtherimproveperformanceonsome
models.Figure7plotsdistributionoftrainedβvaluesfromaMobileNASNet-Amodel(Zophetal.,
2017). Thetrainedβ valuesarespreadoutbetween0and1.5andhaveapeakatβ ≈1,suggesting
thatthemodeltakesadvantageoftheadditionalflexibilityoftrainableβ parameters.
5

|     | Preactivations after training |     |        | β values after training |         |     |     |
| --- | ----------------------------- | --- | ------ | ----------------------- | ------- | --- | --- |
| 10  | 5                             | 0 5 | 10 0.5 | 0.0                     | 0.5 1.0 | 1.5 | 2.0 |
Figure6: Preactivationdistributionafter Figure7:DistributionoftrainedβvaluesofSwish
| trainingofSwishwithβ |     | =1onResNet-32. | onMobileNASNet-A. |     |     |     |     |
| -------------------- | --- | -------------- | ----------------- | --- | --- | --- | --- |
Practically, Swish can be implemented with a single line code change in most deep learning
libraries, such as TensorFlow (Abadi et al., 2016) (e.g., x * tf.sigmoid(beta * x) or
tf.nn.swish(x)ifusingaversionofTensorFlowreleasedafterthesubmissionofthiswork).
Asacautionarynote,ifBatchNorm(Ioffe&Szegedy,2015)isused,thescaleparametershouldbe
set. SomehighlevellibrariesturnoffthescaleparameterbydefaultduetotheReLUfunctionbeing
piecewiselinear,butthissettingisincorrectforSwish. FortrainingSwishnetworks,wefoundthat
slightlyloweringthelearningrateusedtotrainReLUnetworksworkswell.
| 5   | EXPERIMENTS | SWISH |     |     |     |     |     |
| --- | ----------- | ----- | --- | --- | --- | --- | --- |
WITH
We benchmark Swish against ReLU and a number of recently proposed activation functions on
challengingdatasets,andfindthatSwishmatchesorexceedsthebaselinesonnearlyalltasks. The
following sections will describe our experimental settings and results in greater detail. As a sum-
mary,Table3showsSwishincomparisontoeachbaselineactivationfunctionweconsidered(which
aredefinedinthenextsection). TheresultsinTable3areaggregatedbycomparingtheperformance
ofSwishtotheperformanceofdifferentactivationfunctionsappliedtoavarietyofmodels,suchas
InceptionResNet-v2(Szegedyetal.,2017)andTransformer(Vaswanietal.,2017),acrossmultiple
datasets,suchasCIFAR,ImageNet,andEnglish→Germantranslation.1 TheimprovementofSwish
overotheractivationfunctionsisstatisticallysignificantunderaone-sidedpairedsigntest.
|     | Baselines      | ReLU LReLU | PReLU | Softplus | ELU SELU | GELU |     |
| --- | -------------- | ---------- | ----- | -------- | -------- | ---- | --- |
|     | Swish>Baseline | 9 7        | 6     | 6        | 8 8      | 8    |     |
|     | Swish=Baseline | 0 1        | 3     | 2        | 0 1      | 1    |     |
|     | Swish<Baseline | 0 1        | 0     | 1        | 1 0      | 0    |     |
Table 3: The number of models on which Swish outperforms, is equivalent to, or underperforms
eachbaselineactivationfunctionwecomparedagainstinourexperiments.
5.1 EXPERIMENTALSETUP
We compare Swish against several additional baseline activation functions on a variety of models
and datasets. Since many activation functions have been proposed, we choose the most common
activationfunctionstocompareagainst,andfollowtheguidelineslaidoutineachwork:
1Toavoidskewingthecomparison,eachmodeltypeiscomparedjustonce.
Amodelwithmultipleresults
isrepresentedbythemedianofitsresults.Specifically,themodelswithaggregatedresultsare(a)ResNet-164,
WideResNet28-10,andDenseNet100-12acrosstheCIFAR-10andCIFAR-100results,(b)MobileNASNet-A
andInception-ResNet-v2acrossthe3runs,and(c)WMTTransformermodelacrossthe4newstestresults.
6

• LeakyReLU(LReLU)(Maasetal.,2013):
(cid:26) x ifx≥0
f(x)=
αx ifx<0
whereα=0.01. LReLUenablesasmallamountofinformationtoflowwhenx<0.
• ParametricReLU(PReLU)(Heetal.,2015):ThesameformasLReLUbutαisalearnable
| parameter. | Eachchannelhasasharedαwhichisinitializedto0.25. |     |     |
| ---------- | ----------------------------------------------- | --- | --- |
• Softplus(Nair&Hinton,2010): f(x) = log(1+exp(x)). Softplusisasmoothfunction
withpropertiessimilartoSwish,butisstrictlypositiveandmonotonic. Itcanbeviewedas
asmoothversionofReLU.
• ExponentialLinearUnit(ELU)(Clevertetal.,2015):
(cid:26)
x ifx≥0
f(x)=
α(exp(x)−1) ifx<0
whereα=1.0
• ScaledExponentialLinearUnit(SELU)(Klambaueretal.,2017):
(cid:26) x ifx≥0
f(x)=λ
α(exp(x)−1) ifx<0
withα≈1.6733andλ≈1.0507.
• GaussianErrorLinearUnit(GELU)(Hendrycks&Gimpel,2016):f(x)=x·Φ(x),where
Φ(x)isthecumulativedistributionfunctionofthestandardnormaldistribution. GELUis
| anonmonotonicfunctionthathasashapesimilartoSwishwithβ |     |     | =1.4. |
| ----------------------------------------------------- | --- | --- | ----- |
We evaluate both Swish with a trainable β and Swish with a fixed β = 1 (which for simplicity
wecallSwish-1,butitisequivalenttotheSigmoid-weightedLinearUnitofElfwingetal.(2017)).
Notethatourresultsmaynotbedirectlycomparabletotheresultsinthecorrespondingworksdue
todifferencesinourtrainingsetup.
5.2 CIFAR
We first compare Swish to all the baseline activation functions on the CIFAR-10 and CIFAR-100
datasets (Krizhevsky & Hinton, 2009). We follow the same set up used when comparing the acti-
vation functions discovered by the search techniques, and compare the median of 5 runs with the
preactivationResNet-164(Heetal.,2016b),WideResNet28-10(WRN)(Zagoruyko&Komodakis,
2016),andDenseNet100-12(Huangetal.,2017)models.
| Model ResNet              | WRN DenseNet | Model ResNet               | WRN DenseNet |
| ------------------------- | ------------ | -------------------------- | ------------ |
| LReLU 94.2                | 95.6 94.7    | LReLU 74.2                 | 78.0 83.3    |
| PReLU 94.1                | 95.1 94.5    | PReLU 74.5                 | 77.3 81.5    |
| Softplus 94.6             | 94.9 94.7    | Softplus 76.0              | 78.4 83.7    |
| ELU 94.1                  | 94.1 94.4    | ELU 75.0                   | 76.0 80.6    |
| SELU 93.0                 | 93.2 93.9    | SELU 73.2                  | 74.3 80.8    |
| GELU 94.3                 | 95.5 94.8    | GELU 74.7                  | 78.0 83.8    |
| ReLU 93.8                 | 95.3 94.8    | ReLU 74.2                  | 77.8 83.7    |
| Swish-1 94.7              | 95.5 94.8    | Swish-1 75.1               | 78.5 83.8    |
| Swish 94.5                | 95.5 94.8    | Swish 75.1                 | 78.0 83.9    |
| Table4: CIFAR-10accuracy. |              | Table5: CIFAR-100accuracy. |              |
The results in Tables 4 and 5 show how Swish and Swish-1 consistently matches or outperforms
ReLUoneverymodelforbothCIFAR-10andCIFAR-100. Swishalsomatchesorexceedsthebest
baselineperformanceonalmosteverymodel. Importantly,the“bestbaseline”changesbetweendif-
ferentmodels,whichdemonstratesthestabilityofSwishtomatchthesevaryingbaselines. Softplus,
whichissmoothandapproacheszeroononeside,similartoSwish,alsohasstrongperformance.
7

5.3 IMAGENET
Next, webenchmarkSwishagainstthebaselineactivationfunctionsontheImageNet2012classi-
fication dataset (Russakovsky et al., 2015). ImageNet is widely considered one of most important
image classification datasets, consisting of a 1,000 classes and 1.28 million training images. We
evaluateonthevalidationdataset,whichhas50,000images.
We compare all the activation functions on a variety of architectures designed for ImageNet:
Inception-ResNet-v2,Inception-v4,Inception-v3(Szegedyetal.,2017),MobileNet(Howardetal.,
2017), andMobileNASNet-A(Zophetal.,2017). AllthesearchitecturesweredesignedwithRe-
LUs. We again replace the ReLU activation function with different activation functions and train
forafixednumberofsteps,determinedbytheconvergenceoftheReLUbaseline. Foreachactiva-
tionfunction,wetry3differentlearningrateswithRMSProp(Tieleman&Hinton,2012)andpick
the best.2 All networks are initialized with He initialization (He et al., 2015).3 To verify that the
performancedifferencesarereproducible,weruntheInception-ResNet-v2andMobileNASNet-A
experiments3timeswiththebestlearningratefromthefirstexperiment.Weplotthelearningcurves
forMobileNASNet-AinFigure8.
Mobile NASNet-A training curve
|     |     |     |     |     | Model Top-1Acc.(%) |     | Top-5Acc.(%) |     |
| --- | --- | --- | --- | --- | ------------------ | --- | ------------ | --- |
Swish train
Swish valid
| 0.85 | Swish-1 train |     |     |     | LReLU 73.8 | 73.9 74.2 | 91.6 | 91.9 91.9 |
| ---- | ------------- | --- | --- | --- | ---------- | --------- | ---- | --------- |
Swish-1 valid
|     | ReLU train |     |     |     | PReLU 74.6 | 74.7 74.7 | 92.4 | 92.3 92.3 |
| --- | ---------- | --- | --- | --- | ---------- | --------- | ---- | --------- |
ReLU valid
| 0.80     |     |     |     |     | Softplus 74.0 | 74.2 74.2 | 91.6 | 91.8 91.9 |
| -------- | --- | --- | --- | --- | ------------- | --------- | ---- | --------- |
| ycaruccA |     |     |     |     | ELU 74.1      | 74.2 74.2 | 91.8 | 91.8 91.8 |
| 0.75     |     |     |     |     | SELU 73.6     | 73.7 73.7 | 91.6 | 91.7 91.7 |
|          |     |     |     |     | GELU 74.6     | - -       | 92.0 | - -       |
0.70
|     |     |     |     |     | ReLU 73.5 | 73.6 73.8 | 91.4 | 91.5 91.6 |
| --- | --- | --- | --- | --- | --------- | --------- | ---- | --------- |
0.65
|      |     |              |        |        | Swish-1 74.6 | 74.7 74.7 | 92.1 | 92.0 92.0 |
| ---- | --- | ------------ | ------ | ------ | ------------ | --------- | ---- | --------- |
|      |     |              |        |        | Swish 74.9   | 74.9 75.2 | 92.3 | 92.4 92.4 |
| 0.60 | 0   | 50000 100000 | 150000 | 200000 |              |           |      |           |
Training steps
Figure8: TrainingcurvesofMobileNASNet-A Table 6: Mobile NASNet-A on ImageNet, with
onImageNet. Bestviewedincolor 3 different runs ordered by top-1 accuracy. The
additional2GELUexperimentsarestilltraining
atthetimeofsubmission.
Model Top-1Acc.(%) Top-5Acc.(%) Model Top-1Acc.(%) Top-5Acc.(%)
| LReLU    |     | 79.5 79.5 | 79.6 94.7 | 94.7 94.7 | LReLU    | 72.5 |     | 91.0 |
| -------- | --- | --------- | --------- | --------- | -------- | ---- | --- | ---- |
| PReLU    |     | 79.7 79.8 | 80.1 94.8 | 94.9 94.9 | PReLU    | 74.2 |     | 91.9 |
| Softplus |     | 80.1 80.2 | 80.4 95.2 | 95.2 95.3 | Softplus | 73.6 |     | 91.6 |
| ELU      |     | 75.8 79.9 | 80.0 92.6 | 95.0 95.1 | ELU      | 73.9 |     | 91.3 |
| SELU     |     | 79.0 79.2 | 79.2 94.5 | 94.4 94.5 | SELU     | 73.2 |     | 91.0 |
| GELU     |     | 79.6 79.6 | 79.9 94.8 | 94.8 94.9 | GELU     | 73.5 |     | 91.4 |
| ReLU     |     | 79.5 79.6 | 79.8 94.8 | 94.8 94.8 | ReLU     | 72.0 |     | 90.8 |
| Swish-1  |     | 80.2 80.3 | 80.4 95.1 | 95.2 95.2 | Swish-1  | 74.2 |     | 91.6 |
| Swish    |     | 80.2 80.2 | 80.3 95.0 | 95.2 95.0 | Swish    | 74.2 |     | 91.7 |
Table 7: Inception-ResNet-v2 on ImageNet Table8: MobileNetonImageNet.
| with      | 3 different | runs.             | Note that the | ELU      |     |     |     |     |
| --------- | ----------- | ----------------- | ------------- | -------- | --- | --- | --- | --- |
| sometimes |             | has instabilities | at the        | start of |     |     |     |     |
training,whichaccountsforthefirstresult.
The results in Tables 6-10 show strong performance for Swish. On Inception-ResNet-v2, Swish
outperformsReLUbyanontrivial0.5%. Swishperformsespeciallywellonmobilesizedmodels,
2ForsomeofthemodelswithELU,SELU,andPReLU,wetrainwithanadditional3learningrates(soa
totalof6learningrates)becausetheoriginal3learningratesdidnotconverge.
3ForSELU,wetriedbothHeinitializationandtheinitializationrecommendedinKlambaueretal.(2017),
andchoosethebestresultforeachmodelseparately.
8

Model Top-1Acc.(%) Top-5Acc.(%) Model Top-1Acc.(%) Top-5Acc.(%)
| LReLU    | 78.4 | 94.1 |     | LReLU    | 79.3 | 94.7 |
| -------- | ---- | ---- | --- | -------- | ---- | ---- |
| PReLU    | 77.7 | 93.5 |     | PReLU    | 79.3 | 94.4 |
| Softplus | 78.7 | 94.4 |     | Softplus | 79.6 | 94.8 |
| ELU      | 77.9 | 93.7 |     | ELU      | 79.5 | 94.5 |
| SELU     | 76.7 | 92.8 |     | SELU     | 78.3 | 94.5 |
| GELU     | 77.7 | 93.9 |     | GELU     | 79.0 | 94.6 |
| ReLU     | 78.4 | 94.2 |     | ReLU     | 79.2 | 94.6 |
| Swish-1  | 78.7 | 94.2 |     | Swish-1  | 79.3 | 94.7 |
| Swish    | 78.7 | 94.0 |     | Swish    | 79.3 | 94.6 |
Table9: Inception-v3onImageNet. Table10: Inception-v4onImageNet.
with a 1.4% boost on Mobile NASNet-A and a 2.2% boost on MobileNet over ReLU. Swish also
matchesorexceedsthebestperformingbaselineonmostmodels,whereagain,thebestperforming
baselinediffersdependingonthemodel. SoftplusachievesaccuraciescomparabletoSwishonthe
largermodels, butperformsworseonbothmobilesizedmodels. ForInception-v4, thegainsfrom
switchingbetweenactivationfunctionsismorelimited,andSwishslightlyunderperformsSoftplus
and ELU. In general, the results suggest that switching to Swish improves performance with little
additionaltuning.
5.4 MACHINETRANSLATION
WeadditionallybenchmarkSwishonthedomainofmachinetranslation. Wetrainmachinetransla-
tionmodelsonthestandardWMT2014English→Germandataset, whichhas4.5milliontraining
sentences, and evaluate on 4 different newstest sets using the standard BLEU metric. We use the
attentionbasedTransformer(Vaswanietal.,2017)model,whichutilizesReLUsina2-layeredfeed-
forwardnetworkbetweeneachattentionlayer. Wetraina12layer“BaseTransformer”modelwith
2 different learning rates4 for 300K steps, but otherwise use the same hyperparameters as in the
originalwork,suchasusingAdam(Kingma&Ba,2015)tooptimize.
|     | Model                                                       | newstest2013 | newstest2014 | newstest2015 | newstest2016 |     |
| --- | ----------------------------------------------------------- | ------------ | ------------ | ------------ | ------------ | --- |
|     | LReLU                                                       | 26.2         | 27.9         | 29.8         | 33.4         |     |
|     | PReLU                                                       | 26.3         | 27.7         | 29.7         | 33.1         |     |
|     | Softplus                                                    | 23.4         | 23.6         | 25.8         | 29.2         |     |
|     | ELU                                                         | 24.6         | 25.1         | 27.7         | 32.5         |     |
|     | SELU                                                        | 23.7         | 23.5         | 25.9         | 30.5         |     |
|     | GELU                                                        | 25.9         | 27.3         | 29.5         | 33.1         |     |
|     | ReLU                                                        | 26.1         | 27.8         | 29.8         | 33.3         |     |
|     | Swish-1                                                     | 26.2         | 28.0         | 30.1         | 34.0         |     |
|     | Swish                                                       | 26.5         | 27.6         | 30.0         | 33.1         |     |
|     | Table11: BLEUscoreofa12layerTransformeronWMTEnglish→German. |              |              |              |              |     |
Table 11 shows that Swish outperforms or matches the other baselines on machine translation.
Swish-1doesespeciallywellonnewstest2016,exceedingthenextbestperformingbaselineby0.6
BLEUpoints. TheworstperformingbaselinefunctionisSoftplus, demonstratinginconsistencyin
performanceacrossdifferingdomains.Incontrast,Swishconsistentlyperformswellacrossmultiple
domains.
| 6 RELATED | WORK |     |     |     |     |     |
| --------- | ---- | --- | --- | --- | --- | --- |
Swish was found using a variety of automated search techniques. Search techniques have been
utilized in other works to discover convolutional and recurrent architectures (Zoph & Le, 2016;
4WetriedanadditionallearningrateforSoftplus,butfounditdidnotworkwellacrossalllearningrates.
9

Zoph et al., 2017; Real et al., 2017; Cai et al., 2017; Zhong et al., 2017) and optimizers (Bello
etal.,2017). Theuseofsearchtechniquestodiscovertraditionallyhand-designedcomponentsisan
instanceoftherecentlyrevivedsubfieldofmeta-learning(Schmidhuber,1987;Naik&Mammone,
1992;Thrun&Pratt,2012).Meta-learninghasbeenusedtofindinitializationsforone-shotlearning
(Finnetal.,2017;Ravi&Larochelle,2016),adaptablereinforcementlearning(Wangetal.,2016;
Duan et al., 2016), and generating model parameters (Ha et al., 2016). Meta-learning is powerful
becausetheflexibilityderivedfromtheminimalassumptionsencodedleadstoempiricallyeffective
solutions. We take advantage of this property in order to find scalar activation functions, such as
Swish,thathavestrongempiricalperformance.
Whilethisworkfocusesonscalaractivationfunctions,whichtransformonescalartoanotherscalar,
therearemanytypesofactivationfunctionsusedindeepnetworks.Many-to-onefunctions,likemax
pooling,maxout(Goodfellowetal.,2013),andgating(Hochreiter&Schmidhuber,1997;Srivastava
etal.,2015;van denOordetal.,2016;Dauphinetal., 2016;Wuetal.,2016;Miechet al.,2017),
derive their power from combining multiple sources in a nonlinear way. One-to-many functions,
likeConcatenatedReLU(Shangetal.,2016),improveperformancebyapplyingmultiplenonlinear
functionstoasingleinput. Finally,many-to-manyfunctions,suchasBatchNorm(Ioffe&Szegedy,
2015)andLayerNorm(Baetal.,2016), inducepowerfulnonlinearrelationshipsbetweentheirin-
puts.
Mostpriorworkhasfocusedonproposingnewactivationfunctions(Maasetal.,2013;Agostinelli
et al., 2014; He et al., 2015; Clevert et al., 2015; Hendrycks & Gimpel, 2016; Klambauer et al.,
2017;Qiu&Cai,2017;Zhouetal.,2017;Elfwingetal.,2017),butfewstudies,suchasXuetal.
(2015),havesystematicallycompareddifferentactivationfunctions. Tothebestofourknowledge,
thisisthefirststudytocomparescalaractivationfunctionsacrossmultiplechallengingdatasets.
Our study shows that Swish consistently outperforms ReLU on deep models. The strong perfor-
manceofSwishchallengesconventionalwisdomaboutReLU.Hypothesesabouttheimportanceof
thegradientpreservingpropertyofReLUseemunnecessarywhenresidualconnections(Heetal.,
2016a)enabletheoptimizationofverydeepnetworks.Asimilarinsightcanbefoundinthefullyat-
tentionalTransformer(Vaswanietal.,2017),wheretheintricatelyconstructedLSTMcell(Hochre-
iter&Schmidhuber,1997)isnolongernecessarywhenconstant-lengthattentionalconnectionsare
used. Architecturalimprovementslessentheneedforindividualcomponentstopreservegradients.
7 CONCLUSION
Inthiswork,weutilizedautomaticsearchtechniquestodiscovernovelactivationfunctionsthathave
strongempiricalperformance.Wethenempiricallyvalidatedthebestdiscoveredactivationfunction,
which we call Swish and is defined as f(x) = x · sigmoid(βx). Our experiments used models
andhyperparametersthatweredesignedforReLUandjustreplacedtheReLUactivationfunction
with Swish; even this simple, suboptimal procedure resulted in Swish consistently outperforming
ReLU and other activation functions. We expect additional gains to be made when these models
and hyperparameters are specifically designed with Swish in mind. The simplicity of Swish and
its similarity to ReLU means that replacing ReLUs in any network is just a simple one line code
change.
ACKNOWLEDGEMENTS
WethankEstebanReal,GeoffreyHinton,IrwanBello,JaschaSohl-Dickstein,JonShlens,Kathryn
Rough, Mohammad Norouzi, Navdeep Jaitly, Niki Parmar, Sam Smith, Simon Kornblith, Vijay
Vasudevan,andtheGoogleBrainteamforhelpwiththisproject.
REFERENCES
Mart´ınAbadi,PaulBarham,JianminChen,ZhifengChen,AndyDavis,JeffreyDean,MatthieuDevin,Sanjay
Ghemawat,GeoffreyIrving,MichaelIsard,etal.Tensorflow:Asystemforlarge-scalemachinelearning.In
USENIXSymposiumonOperatingSystemsDesignandImplementation,volume16,pp.265–283,2016.
Forest Agostinelli, Matthew Hoffman, Peter Sadowski, and Pierre Baldi. Learning activation functions to
improvedeepneuralnetworks. arXivpreprintarXiv:1412.6830,2014.
10

Jimmy Lei Ba, Jamie Ryan Kiros, and Geoffrey E Hinton. Layer normalization. In Advances in Neural
InformationProcessingSystems,2016.
Irwan Bello, Barret Zoph, Vijay Vasudevan, and Quoc V Le. Neural optimizer search with reinforcement
learning. InInternationalConferenceonMachineLearning,pp.459–468,2017.
Han Cai, Tianyao Chen, Weinan Zhang, Yong Yu, and Jun Wang. Reinforcement learning for architecture
searchbynetworktransformation. arXivpreprintarXiv:1707.04873,2017.
Djork-Arne´ Clevert, ThomasUnterthiner, andSeppHochreiter. Fastandaccuratedeepnetworklearningby
exponentiallinearunits(elus). arXivpreprintarXiv:1511.07289,2015.
YannNDauphin,AngelaFan,MichaelAuli,andDavidGrangier.Languagemodelingwithgatedconvolutional
networks. arXivpreprintarXiv:1612.08083,2016.
YanDuan,JohnSchulman,XiChen,PeterLBartlett,IlyaSutskever,andPieterAbbeel. Rl2: Fastreinforce-
mentlearningviaslowreinforcementlearning. arXivpreprintarXiv:1611.02779,2016.
Stefan Elfwing, Eiji Uchibe, and Kenji Doya. Sigmoid-weighted linear units for neural network function
approximationinreinforcementlearning. arXivpreprintarXiv:1702.03118,2017.
Chelsea Finn, Pieter Abbeel, and Sergey Levine. Model-agnostic meta-learning for fast adaptation of deep
networks. arXivpreprintarXiv:1703.03400,2017.
IanJGoodfellow,DavidWarde-Farley,MehdiMirza,AaronCourville,andYoshuaBengio.Maxoutnetworks.
InInternationalConferenceonMachineLearning,2013.
DavidHa,AndrewDai,andQuocVLe. Hypernetworks. arXivpreprintarXiv:1609.09106,2016.
RichardHRHahnloser, RahulSarpeshkar, MishaAMahowald, RodneyJDouglas, andHSebastianSeung.
Digitalselectionandanalogueamplificationcoexistinacortex-inspiredsiliconcircuit. Nature,405(6789):
947,2000.
KaimingHe,XiangyuZhang,ShaoqingRen,andJianSun. Delvingdeepintorectifiers: Surpassinghuman-
levelperformanceonimagenetclassification. InProceedingsoftheIEEEinternationalconferenceoncom-
putervision,pp.1026–1034,2015.
KaimingHe,XiangyuZhang,ShaoqingRen,andJianSun. Deepresiduallearningforimagerecognition. In
ProceedingsoftheIEEEconferenceoncomputervisionandpatternrecognition,pp.770–778,2016a.
KaimingHe,XiangyuZhang,ShaoqingRen,andJianSun. Identitymappingsindeepresidualnetworks. In
EuropeanConferenceonComputerVision,pp.630–645.Springer,2016b.
Dan Hendrycks and Kevin Gimpel. Bridging nonlinearities and stochastic regularizers with gaussian error
linearunits. arXivpreprintarXiv:1606.08415,2016.
SeppHochreiterandJu¨rgenSchmidhuber. Longshort-termmemory. NeuralComputation,9(8):1735–1780,
1997.
AndrewGHoward, MenglongZhu, BoChen, DmitryKalenichenko, WeijunWang, TobiasWeyand, Marco
Andreetto,andHartwigAdam. Mobilenets: Efficientconvolutionalneuralnetworksformobilevisionap-
plications. arXivpreprintarXiv:1704.04861,2017.
GaoHuang,ZhuangLiu,KilianQWeinberger,andLaurensvanderMaaten. Denselyconnectedconvolutional
networks. InConferenceonComputerVisionandPatternRecognition,2017.
Sergey Ioffe and Christian Szegedy. Batch normalization: Accelerating deep network training by reducing
internalcovariateshift. InInternationalConferenceonMachineLearning,pp.448–456,2015.
Kevin Jarrett, Koray Kavukcuoglu, Yann LeCun, et al. What is the best multi-stage architecture for object
recognition? In2009IEEE12thInternationalConferenceonComputerVision,2009.
DiederikKingmaandJimmyBa. Adam: Amethodforstochasticoptimization. InInternationalConference
onLearningRepresentations,2015.
Gu¨nter Klambauer, Thomas Unterthiner, Andreas Mayr, and Sepp Hochreiter. Self-normalizing neural net-
works. arXivpreprintarXiv:1706.02515,2017.
AlexKrizhevskyandGeoffreyHinton.Learningmultiplelayersoffeaturesfromtinyimages.Technicalreport,
Technicalreport,UniversityofToronto,2009.
11

AlexKrizhevsky,IlyaSutskever,andGeoffreyEHinton.Imagenetclassificationwithdeepconvolutionalneural
networks. InAdvancesinNeuralInformationProcessingSystems,pp.1097–1105,2012.
AndrewLMaas,AwniYHannun,andAndrewYNg.Rectifiernonlinearitiesimproveneuralnetworkacoustic
models. InInternationalConferenceonMachineLearning,volume30,2013.
AntoineMiech,IvanLaptev,andJosefSivic. Learnablepoolingwithcontextgatingforvideoclassification.
arXivpreprintarXiv:1706.06905,2017.
DevangKNaikandRJMammone. Meta-neuralnetworksthatlearnbylearning. InNeuralNetworks,1992.
IJCNN.,InternationalJointConferenceon,volume1,pp.437–442.IEEE,1992.
VinodNairandGeoffreyEHinton. Rectifiedlinearunitsimproverestrictedboltzmannmachines. InInterna-
tionalConferenceonMachineLearning,2010.
Giambattista Parascandolo, Heikki Huttunen, and Tuomas Virtanen. Taming the waves: sine as activation
functionindeepneuralnetworks. 2016.
Suo Qiu and Bolun Cai. Flexible rectified linear units for improving convolutional neural networks. arXiv
preprintarXiv:1706.08098,2017.
SachinRaviandHugoLarochelle. Optimizationasamodelforfew-shotlearning. 2016.
Esteban Real, Sherry Moore, Andrew Selle, Saurabh Saxena, Yutaka Leon Suematsu, Quoc Le, and Alex
Kurakin. Large-scaleevolutionofimageclassifiers. arXivpreprintarXiv:1703.01041,2017.
Olga Russakovsky, Jia Deng, Hao Su, Jonathan Krause, Sanjeev Satheesh, Sean Ma, Zhiheng Huang, An-
drejKarpathy,AdityaKhosla,MichaelBernstein,etal. Imagenetlargescalevisualrecognitionchallenge.
InternationalJournalofComputerVision,115(3):211–252,2015.
Jurgen Schmidhuber. Evolutionary principles in self-referential learning. On learning how to learn: The
meta-meta-...hook.)Diplomathesis,Institutf.Informatik,Tech.Univ.Munich,1987.
JohnSchulman,FilipWolski,PrafullaDhariwal,AlecRadford,andOlegKlimov.Proximalpolicyoptimization
algorithms. arXivpreprintarXiv:1707.06347,2017.
WenlingShang,KihyukSohn,DiogoAlmeida,andHonglakLee. Understandingandimprovingconvolutional
neuralnetworksviaconcatenatedrectifiedlinearunits. InInternationalConferenceonMachineLearning,
pp.2217–2225,2016.
Rupesh Kumar Srivastava, Klaus Greff, and Ju¨rgen Schmidhuber. Highway networks. arXiv preprint
arXiv:1505.00387,2015.
ChristianSzegedy,VincentVanhoucke,SergeyIoffe,JonShlens,andZbigniewWojna. Rethinkingtheincep-
tionarchitectureforcomputervision.InTheIEEEConferenceonComputerVisionandPatternRecognition
(CVPR),June2016.
ChristianSzegedy,SergeyIoffe,VincentVanhoucke,andAlexanderAAlemi. Inception-v4,inception-resnet
andtheimpactofresidualconnectionsonlearning. InAAAI,pp.4278–4284,2017.
SebastianThrunandLorienPratt. Learningtolearn. SpringerScience&BusinessMedia,2012.
TijmenTielemanandGeoffreyHinton. Lecture6.5-rmsprop: Dividethegradientbyarunningaverageofits
recentmagnitude. COURSERA:Neuralnetworksformachinelearning,4(2):26–31,2012.
AaronvandenOord,NalKalchbrenner,LasseEspeholt,OriolVinyals,AlexGraves,etal. Conditionalimage
generationwithpixelcnndecoders. InAdvancesinNeuralInformationProcessingSystems,pp.4790–4798,
2016.
AshishVaswani,NoamShazeer,NikiParmar,JakobUszkoreit,LlionJones,AidanNGomez,LukaszKaiser,
and Illia Polosukhin. Attention is all you need. In Advances in Neural Information Processing Systems,
2017.
Jane X Wang, Zeb Kurth-Nelson, Dhruva Tirumala, Hubert Soyer, Joel Z Leibo, Remi Munos, Charles
Blundell, Dharshan Kumaran, and Matt Botvinick. Learning to reinforcement learn. arXiv preprint
arXiv:1611.05763,2016.
Yuhuai Wu, Saizheng Zhang, Ying Zhang, Yoshua Bengio, and Ruslan R Salakhutdinov. On multiplicative
integration with recurrent neural networks. In Advances in Neural Information Processing Systems, pp.
2856–2864,2016.
12

BingXu,NaiyanWang,TianqiChen,andMuLi. Empiricalevaluationofrectifiedactivationsinconvolutional
| network. arXivpreprintarXiv:1505.00853,2015. |     |     |     |
| -------------------------------------------- | --- | --- | --- |
British Machine Vision Conference,
| Sergey Zagoruyko | and Nikos Komodakis. | Wide residual | networks. In |
| ---------------- | -------------------- | ------------- | ------------ |
2016.
ZhaoZhong,JunjieYan,andCheng-LinLiu. Practicalnetworkblocksdesignwithq-learning. arXivpreprint
arXiv:1708.05552,2017.
GuoruiZhou,ChengruSong,XiaoqiangZhu,XiaoMa,YanghuiYan,XingyaDai,HanZhu,JunqiJin,Han
Li,andKunGai. Deepinterestnetworkforclick-throughrateprediction. arXivpreprintarXiv:1706.06978,
2017.
BarretZophandQuocVLe. Neuralarchitecturesearchwithreinforcementlearning. InInternationalConfer-
enceonLearningRepresentations,2016.
BarretZoph,VijayVasudevan,JonathonShlens,andQuocVLe. Learningtransferablearchitecturesforscal-
| ableimagerecognition. | arXivpreprintarXiv:1707.07012,2017. |     |     |
| --------------------- | ----------------------------------- | --- | --- |
13
