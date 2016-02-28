# tennis-prediction
Implementation of the paper "Machine Learning for the Prediction of Professional Tennis Matches" (Sipko, 2015).

The plan
First, we need the data, that is information about tournaments (ATP only), players, and matches, with detailed statistics for each of them. The best source is the Oncourt database, which you can download from their website. Otherwise, you can use JeffSackmann's data, which is very good but lacks winners, unforced errors, net approaches and serve speed. Regarding betting data, we used the odds provided by tennis-data.co.uk.
Next, we need to parse the data; in other words, we will read the .csv files into the program, and store the data in some custom-defined classes (Tournament, Player, Match, Set, Statistics). Having got the data in our program, now we need to produce the features we need. For this step, we followed the paper, which basically suggests to take two averages, weighted by time and surface, for each player, and subtract them. We also implemented the *common opponent* model. The program writes the features for every match to a .csv file.
Finally, we will find a pattern in the data using a learning algorithm. The paper suggests logistic regression and neural network. We also used SVM and decision trees.

The results
We get 65% accuracy on the training set, that is we correctly predict the outcome for 65 matches every 100. The result is not bad, but we couldn't compare it with the result the author achieved because he doesn't mention it in the paper. It does mention, however, the ROI of the betting strategy based on the Kelly criterion. The author got a positive ROI, but our backtests since 2004 show a negative profit.
