#!/usr/bin/env python
# coding: utf-8

# In[6]:


import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import os
import tensorflow as tf

# get_ipython().run_line_magic('matplotlib', 'inline')
#print (os.environ['CONDA_DEFAULT_ENV'])
#print (tf.VERSION)

# ## Get the dataset and prepare it for analysis and model

# #### Set the index to date

# In[78]:


#series_file = 'GBPUSD.csv'
series_file = '#US100_M9.csv'
df = pd.read_csv(series_file)
#df = pd.read_csv('#USNDAQ100.csv')

df['time'] = pd.to_datetime(df["time"])
df_idx = df.set_index(["time"], drop=True)
df_idx.head(5)


# #### Flip the dataframe

# In[79]:


df_idx = df_idx.sort_index(axis=1, ascending=True)
df_idx = df_idx.iloc[::-1]


# #### Plot the data
# In[80]:

data = df_idx[['close']]
data.plot(y='close')

# In[81]:

#### Create Histogram

# In[86]:

#import seaborn as sns

#plt.figure(figsize=(50,10))
#hist_count = df_idx['close'].value_counts(bins=100).sort_index(axis=0) #.sort_values(ascending=True)

##ax = sns.barplot(x=hist_count.index, y=hist_count.values)
##df['close'].value_counts().plot(kind='barh')
##hist_count.plot(kind='barh')
#plt.hist(df['close'], bins=200)

#plt.ylabel('Count')
#plt.xlabel('close price')
#plt.show()


# In[72]:


diff = data.index.values[-1] - data.index.values[0]
days = diff.astype('timedelta64[D]')
days = days / np.timedelta64(1, 'D')
#years = int(days/365)
print("Total data: %d days"%days)
# print("80 percent data = 1980 to %d"%(1980 + int(0.8*years)))


# #### Create training and testing data

# In[71]:


#split_date = pd.Timestamp('2019.02.28 01:00', tz=None)
split_date = pd.Timestamp('2019.03.15 01:00')

# pd.Timestamp('2019.03.01')
#print (split_date)
train = data.loc[:split_date]
test = data.loc[split_date:]

ax = train.plot(figsize=(10,12))
test.plot(ax=ax)
plt.legend(['train', 'test'])
plt.show()


# #### Normalize the dataset

# In[72]:


from sklearn.preprocessing import MinMaxScaler
sc = MinMaxScaler()
train_sc = sc.fit_transform(train)
test_sc = sc.transform(test)


# In[73]:


train_sc_df = pd.DataFrame(train_sc, columns=['Y'], index=train.index)
test_sc_df = pd.DataFrame(test_sc, columns=['Y'], index=test.index)

for s in range(1,2):
    train_sc_df['X_{}'.format(s)] = train_sc_df['Y'].shift(s)
    test_sc_df['X_{}'.format(s)] = test_sc_df['Y'].shift(s)

X_train = train_sc_df.dropna().drop('Y', axis=1)
y_train = train_sc_df.dropna().drop('X_1', axis=1)

X_test = test_sc_df.dropna().drop('Y', axis=1)
y_test = test_sc_df.dropna().drop('X_1', axis=1)

X_train = X_train.values
y_train = y_train.values

X_test = X_test.values
y_test = y_test.values


# In[74]:


print('Train size: (%d x %d)'%(X_train.shape[0], X_train.shape[1]))
print('Test size: (%d x %d)'%(X_test.shape[0], X_test.shape[1]))


# ## Setup baseline model of SVM Regressor

# In[75]:


from sklearn.svm import SVR
regressor = SVR(kernel='rbf')


# In[76]:


regressor.fit(X_train, y_train)
y_pred = regressor.predict(X_test)


# In[77]:


plt.plot(y_test)
plt.plot(y_pred)


# ### 1 hidden layer with 1 neuron

# In[78]:


from sklearn.metrics import r2_score

def adj_r2_score(r2, n, k):
    return 1-((1-r2)*((n-1)/(n-k-1)))

r2_test = r2_score(y_test, y_pred)
print("R-squared is: %f"%r2_test)


# ## Build a Neural Network

# In[79]:

if tf.__version__ < "2.0.0a":
    from keras.models import Sequential
    from keras.layers import Dense
    from keras.optimizers import Adam
    import keras.backend as K
else :
    from tensorflow.keras.models import Sequential 
    from tensorflow.keras.layers import Dense 
    from tensorflow.keras.optimizers import Adam 
    import tensorflow.keras.backend as K 
# end if

# In[80]:

K.clear_session()
model = Sequential()
model.add(Dense(1, input_shape=(X_test.shape[1],), activation='tanh', kernel_initializer='lecun_uniform'))
model.compile(optimizer=Adam(lr=0.001), loss='mean_squared_error')
model.fit(X_train, y_train, batch_size=16, epochs=20, verbose=1)


# In[81]:


y_pred = model.predict(X_test)
plt.plot(y_test)
plt.plot(y_pred)
print('R-Squared: %f'%(r2_score(y_test, y_pred)))


# #### 2 Hidden Layers with 50 neurons each and ReLU activation function

# In[82]:


K.clear_session()
model = Sequential()
model.add(Dense(50, input_shape=(X_test.shape[1],), activation='relu', kernel_initializer='lecun_uniform'))
model.add(Dense(50, input_shape=(X_test.shape[1],), activation='relu'))
model.add(Dense(1))
model.compile(optimizer=Adam(lr=0.001), loss='mean_squared_error')
model.fit(X_train, y_train, batch_size=16, epochs=20, verbose=1)


# In[83]:

y_pred = model.predict(X_test)
plt.plot(y_test)
plt.plot(y_pred)
plt.show()


print('R-Squared: %f'%(r2_score(y_test, y_pred)))






